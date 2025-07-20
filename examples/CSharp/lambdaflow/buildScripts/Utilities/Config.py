from collections.abc import Mapping

from Utilities.Utilities import *

class Config():
    def __init__(self, path):
        normalized_path = Normalize(path)

        with open(normalized_path, encoding="utf-8") as f:
            self._data = json.load(f)

    def __eq__(self, other):
        return self._data == other._data

    def __str__(self):
        return f"{self._data}"

    def Get(self, key, default=None, sep="."):
        node = self._data
        for k in key.split(sep):
            if isinstance(node, Mapping) and k in node:
                node = node[k]
            else:
                return default

        return node