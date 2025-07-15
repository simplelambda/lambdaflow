import secrets

from Utilities import *

def integrity_modify_framework(plat):

	inject_global_variable("lambdaflow/TMP/lambdaflow/source/Utilities.cs", "securityMode", "SecurityMode.INTEGRITY")


	# ----- CREATION OF INTEGRITY.JSON -----

	print_banner("Generating integrity.json...", banner_type="info")

	manifest = {
		"backend.pak": sha512(normalizePath("lambdaflow/TMP/backend.pak")),
		"frontend.pak": sha512(normalizePath("lambdaflow/TMP/frontend.pak"))
	}

	with open(normalizePath("lambdaflow/TMP/integrity.json"), "w", encoding="utf-8") as f:
		json.dump(manifest, f, indent=2)


    # ----- GENERATE AES-GCM KEYS FOR INTEGRITY.JSON AND CONFIG.JSON -----

	print_banner("Injecting sintegrity.json and config.json random keys", banner_type="info")

	integrity_key_bytes = secrets.token_bytes(32)
	integrityKey = ', '.join(f'0x{b:02X}' for b in integrity_key_bytes)

	config_key_bytes = secrets.token_bytes(32)
	configKey = ', '.join(f'0x{b:02X}' for b in config_key_bytes)

	inject_global_variable("lambdaflow/TMP/lambdaflow/source/Utilities.cs", "RandomIntegrityKey", f"{{ {integrityKey} }}")
	inject_global_variable("lambdaflow/TMP/lambdaflow/source/Utilities.cs", "RandomConfigKey", f"{{ {configKey} }}")


	# ----- ENCRYPT INTEGRITY.JSON AND CONFIG.JSON

	print_banner("Encrypting integrity.json and config.json", banner_type="info")

	encrypt_file_aes_gcm(integrity_key_bytes, "lambdaflow/TMP/integrity.json")
	encrypt_file_aes_gcm(config_key_bytes, "lambdaflow/TMP/config.json")
	
	# ----- INJECT PUBLIC KEY -----

	print_banner("Injecting public key", banner_type="info")

	public_key = ""
	with open(normalizePath("lambdaflow/TMP/public.pub"), "r", encoding="utf-8") as f:
		public_key = f.read()

	inject_global_variable(f"lambdaflow/TMP/lambdaflow/source/Security/Signers/{plat.capitalize()}Signer.cs", "PublicKeyPem", f"@\"{public_key}\"")