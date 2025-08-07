import secrets

from SecurityStrategies.Strategy import Strategy
from Utilities.Utilities         import *

class Integrity(Strategy):
	def Apply(self):
		# ----- CREATION OF INTEGRITY DATA -----

		log("Creating integrity settings", banner_type="info")

		manifest = f"""
			{{
				""backend.pak"": ""{sha512(Normalize("lambdaflow/TMP/backend.pak"))}"",
				""frontend.pak"": ""{sha512(Normalize("lambdaflow/TMP/frontend.pak"))}""
			}}
		"""

		inject_global_variable("lambdaflow/source/Config.cs", "Integrity", f"@\"{manifest}\"")
	
		# ----- INJECT PUBLIC KEY -----

		log("Injecting public key", banner_type="info")

		public_key = ""
		with open(Normalize("lambdaflow/TMP/public.pub"), "r", encoding="utf-8") as f:
			public_key = f.read()

		inject_global_variable("lambdaflow/source/Config.cs", "PublicKeyPem", f"@\"{public_key}\"")