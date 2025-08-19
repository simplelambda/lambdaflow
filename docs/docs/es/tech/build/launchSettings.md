# Documentaci�n t�cnica

En esta secci�n se explica detalladamente que es el fichero 'launchSettings.json' y porqu� existe,
adem�s comprendiendo este fichero, se podr�n crear nuevos modos de compilaci�n personalizados.

---

## �Qu� es launchSettings.json?

Como ya se ha comentado en la secci�n [Introducci�n de la documentaci�n t�cnica](../index.md), por ahora LambdaFlow est� fuertemente
ligado a [`Visual Studio`](https://visualstudio.microsoft.com/), por tanto se requieren ficheros que permitan a los desarrolladores
utilizar este IDE correctamente.

`launchSettings.json` es un fichero de configuraci�n que permite a [`Visual Studio`](https://visualstudio.microsoft.com/) conocer los modos de compilaci�n disponibles, 
este archivo define **perfiles**. En este caso, define dos perfiles de compilaci�n que al ejecutarse lanzan un script de Python para realizar la
compilaci�n.

---

## Analizando launchSettings.json

A continuaci�n se muestra el fichero launchSettings.json por defento:

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

Se puede observar que en el json hay una propiedad `profiles`, esta es la que contiene todos los perfiles de compilaci�n que est�n disponibles para los desarrolladores.

En este caso se definen los dos perfiles que ya se mencionaron en la secci�n de [Introducci�n de la Compilaci�n](index.md), `Build` y `Run`. Adem�s ambos perfiles son esencialmente
iguales, solo hay un cambio m�nimo entre ambos, por tanto se explicar� �nicamente el perfil `Build`.

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

El nombre del perfil es el de la propiedad, este ser� el nombre que aparecer� en [`Visual Studio`](https://visualstudio.microsoft.com/).

A continuaci�n se muestra una tabla que explica cada una de las propiedades del perfil:


| Propiedad            |                                                                                          Descripci�n                                                                                                 |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `commandName`        | Define el tipo de comando que se ejecutar�, con el valor "Executable" se indica que se lanzar� un ejecutable externo.                                                                                |
| `executablePath`     | Especifica la ruta del ejecutable que se ejecutar�, en este caso es el int�rprete de Python. Es importante que Python est� instalado y en el PATH del sistema. Para que el sistema sepa que es "py". |
| `commandLineArgs`    | Son los argumentos que se pasar�n al ejecutable, en concreto a "py", aqu� se indica el script de Python a ejecutar y se le pasa el argumento `$(Configuration)` que se explica m�s adelante.         |
| `workingDirectory`   | Especifica el directorio de trabajo desde donde se ejecutar� el comando, por defecto se utiliza el directorio base del proyecto, representado por `$(ProjectDir)`.                                   |
| `console`            | Define el tipo de consola que se utilizar�, "internalConsole" indica que se utilizar� la consola interna de [`Visual Studio`](https://visualstudio.microsoft.com/).                                  |

Si quieres conocer m�s sobre las propiedades de `launchSettings.json`, puedes consultar la [documentaci�n oficial de Microsoft](https://learn.microsoft.com/en-us/visualstudio/containers/container-launch-settings?view=vs-2022).

Sobre las propiedades mencionadas hay 2 cuestiones a destacar:

1. La propiedad `commandLineArgs` contiene el argumento `$(Configuration)`, esta variable se utiliza por defecto en [`Visual Studio`](https://visualstudio.microsoft.com/). Cada perfil de compilaci�n tiene "Submodos",
   los m�s tpicos son `Debug` y `Release`, que vienen definidos por defecto en el IDE. Cuando el desarrollador selecciona el perfil de compilaci�n tambi�n puede elegir si utilizar el modo `Debug` o `Release`, esa elecci�n
   ser� la que se almacenar� en `$(Configuration)`. Por tanto, si el desarrollador selecciona el perfil `Build` y el modo `Debug`, el valor de `$(Configuration)` ser� `Debug`, esta informaci�n se le pasar� al script de Python
   `lambdaflow/buildScripts/build.py` como argumento, permitiendo que el script sepa en qu� modo se est� compilando.

2. En la misma propiedad `commandLineArgs` se indica el script de Python a ejecutar es `lambdaflow/buildScripts/build.py`. Este script es el encargado de realizar la compilaci�n del proyecto, 
   y es el que se ejecutar� cuando se pulse el bot�n de "Play" en [`Visual Studio`](https://visualstudio.microsoft.com/). Aunque este script cambia entre los perfiles `Build` y `Run`, pues el pipeline de compilaci�n es diferente,

---