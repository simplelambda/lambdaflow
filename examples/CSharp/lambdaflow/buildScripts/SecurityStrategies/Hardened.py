from Utilities import *

def hardened_modify_framework(plat):
	inject_global_variable("lambdaflow/TMP/lambdaflow/source/Utilities.cs", "securityMode", "SecurityMode.HARDENED")
