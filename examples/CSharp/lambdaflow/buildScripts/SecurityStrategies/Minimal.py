from Utilities import *

def minimal_modify_framework(plat):
	inject_global_variable("lambdaflow/TMP/lambdaflow/source/Utilities.cs", "securityMode", "SecurityMode.MINIMAL")