# Documentación técnica

En esta sección se explica detalladamente que es el fichero 'launchSettings.json' y porqué existe,
además comprendiendo este fichero, se podrán crear nuevos modos de compilación personalizados.

---

## ¿Qué es launchSettings.json?

Como ya se ha comentado en la sección [Introducción de la documentación técnica](../index.md), por ahora LambdaFlow está fuertemente
ligado a [`Visual Studio`](https://visualstudio.microsoft.com/), por tanto se requieren ficheros que permitan a los desarrolladores
utilizar este IDE correctamente.

`launchSettings.json` es un fichero de configuración que permite a [`Visual Studio`](https://visualstudio.microsoft.com/) conocer los modos de compilación disponibles, 
este archivo define **perfiles**. En este caso, define dos perfiles de compilación que al ejecutarse lanzan un script de Python para realizar la
compilación.

---

## Analizando launchSettings.json

A continuación se muestra el fichero launchSettings.json por defento:

```json
{
  "profiles": {
    "Build": {
      "commandName": "Executable",
      "executablePath": "py",
      "commandLineArgs": "lambdaflow/buildScripts/build.py $(Configuration)",
      "workingDirectory": "$(ProjectDir)",
      "console": "internalConsole"
    },

    "Run": {
      "commandName": "Executable",
      "executablePath": "py",
      "commandLineArgs": "lambdaflow/buildScripts/run.py $(Configuration)",
      "workingDirectory": "$(ProjectDir)",
      "console": "internalConsole"
    }
  }
}
```

Se puede observar que en el json hay una propiedad `profiles`, esta es la que contiene todos los perfiles de compilación que están disponibles para los desarrolladores.

En este caso se definen los dos perfiles que ya se mencionaron en la sección de [Introducción de la Compilación](index.md), `Build` y `Run`. Además ambos perfiles son esencialmente
iguales, solo hay un cambio mínimo entre ambos, por tanto se explicará únicamente el perfil `Build`.

Vamos a centrarnos en el perfil `Build`:

```json
"Build": {
    "commandName": "Executable",
    "executablePath": "py",
    "commandLineArgs": "lambdaflow/buildScripts/build.py $(Configuration)",
    "workingDirectory": "$(ProjectDir)",
    "console": "internalConsole"
}
```

El nombre del perfil es el de la propiedad, este será el nombre que aparecerá en [`Visual Studio`](https://visualstudio.microsoft.com/).

A continuación se muestra una tabla que explica cada una de las propiedades del perfil:


| Propiedad            |                                                                                          Descripción                                                                                                 |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `commandName`        | Define el tipo de comando que se ejecutará, con el valor "Executable" se indica que se lanzará un ejecutable externo.                                                                                |
| `executablePath`     | Especifica la ruta del ejecutable que se ejecutará, en este caso es el intérprete de Python. Es importante que Python esté instalado y en el PATH del sistema. Para que el sistema sepa que es "py". |
| `commandLineArgs`    | Son los argumentos que se pasarán al ejecutable, en concreto a "py", aquí se indica el script de Python a ejecutar y se le pasa el argumento `$(Configuration)` que se explica más adelante.         |
| `workingDirectory`   | Especifica el directorio de trabajo desde donde se ejecutará el comando, por defecto se utiliza el directorio base del proyecto, representado por `$(ProjectDir)`.                                   |
| `console`            | Define el tipo de consola que se utilizará, "internalConsole" indica que se utilizará la consola interna de [`Visual Studio`](https://visualstudio.microsoft.com/).                                  |

Si quieres conocer más sobre las propiedades de `launchSettings.json`, puedes consultar la [documentación oficial de Microsoft](https://learn.microsoft.com/en-us/visualstudio/containers/container-launch-settings?view=vs-2022).

Sobre las propiedades mencionadas hay 2 cuestiones a destacar:

1. La propiedad `commandLineArgs` contiene el argumento `$(Configuration)`, esta variable se utiliza por defecto en [`Visual Studio`](https://visualstudio.microsoft.com/). Cada perfil de compilación tiene "Submodos",
   los más tpicos son `Debug` y `Release`, que vienen definidos por defecto en el IDE. Cuando el desarrollador selecciona el perfil de compilación también puede elegir si utilizar el modo `Debug` o `Release`, esa elección
   será la que se almacenará en `$(Configuration)`. Por tanto, si el desarrollador selecciona el perfil `Build` y el modo `Debug`, el valor de `$(Configuration)` será `Debug`, esta información se le pasará al script de Python
   `lambdaflow/buildScripts/build.py` como argumento, permitiendo que el script sepa en qué modo se está compilando.

2. En la misma propiedad `commandLineArgs` se indica el script de Python a ejecutar es `lambdaflow/buildScripts/build.py`. Este script es el encargado de realizar la compilación del proyecto, 
   y es el que se ejecutará cuando se pulse el botón de "Play" en [`Visual Studio`](https://visualstudio.microsoft.com/). Aunque este script cambia entre los perfiles `Build` y `Run`, pues el pipeline de compilación es diferente,

---