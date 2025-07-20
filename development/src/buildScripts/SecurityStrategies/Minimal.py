from SecurityStrategies.Strategy import Strategy
from Utilities.Utilities         import *

class Minimal(Strategy):
	def Apply(self):
		inject_global_variable("lambdaflow/TMP/lambdaflow/source/Config.cs", "SecurityMode", "SecurityMode.MINIMAL")