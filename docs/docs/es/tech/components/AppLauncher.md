# AppLauncher

La clase `AppLauncher` es la `puerta de entrada` a la aplicación y el coordinador que controla el ciclo de vida completo. Se organiza en métodos independientes para cumplir el principio de responsabilidad única y facilitar el mantenimiento:

- Métodos públicos:

    - `AppLauncher(Config config)`

        - **Función**: recibe y almacena la configuración (Modos de seguridad, rutas, flags...)

    - `Run()`

        - **Función**: inicia el proceso de arranque y, al finalizar la inicialización, entra en el bucle de interfaz de usuario (UI loop).

- Métodos privados:

    Todos los métodos privados de la clase son llamados dentro del método `Run()`. 

    - `SetupSecurity()`

        - **Función**: aplica las políticas de seguridad antes de cargar cualquier otro componente.

## 1. Modos de seguridad (Estrategias)

|    Modo   |                                       Descripción                                              |
|-----------|------------------------------------------------------------------------------------------------|
| NONE      | `Sin comprobaciones`. Ideal para prototipos o herramientas locales que no requieren seguridad. |
| DEV       | `Advertencias` en consola, salta checks pesados para desarrollo.                               |
| INTEGRITY | `Hash + firma` de recursos y binarios. Bloquea si hay manipulación.                            |

## 2. Firma a nivel de sistema operativo

Para garantizar la integridad y autenticidad del ejecutable y los recursos empaquetados, Lambdaflow recomienda usar el mecanismo de firma nativo de cada plataforma. A continuación se describen los métodos, requisitos y opciones para firmar en Windows, Linux y Android, así como consideraciones cross-platform.

🔷 Windows (Authenticode)

- **Qué hace**: incrusta un sello de firma digital en el archivo PE (.exe, .dll). **Windows verifica la firma al ejecutar o instalar**.

- Requisitos:

    - `Certificado de firma de código` en formato PFX (emitido por CA reconocida o interno).

    - Herramienta `signtool.exe` (parte del Windows SDK) o `osslsigncode` en Linux.

🔷 Linux (Packages firmados con GPG)

- **Qué hace**: firma el paquete (.deb, .rpm) o genera una firma detached (.tar.gz.sig). **El gestor de paquetes valida la firma al instalar**.

- Requisitos:

    - `Clave GPG privada para firmar`.

    - `Configuración de repositorio APT/RPM` con clave pública en el sistema del usuario.

    - En Windows puedes usar `Gpg4win` para firmar paquetes Linux.

🔷 Android (APK Signing)

- **Qué hace**: firma digitalmente el APK para autorizar la instalación en dispositivos Android.

- Requisitos:

    - `Keystore Java (.jks)` con clave privada.

    - Herramienta `apksigner` (parte del Android SDK Build-Tools) o `jarsigner`.


> `apksigner` y `jarsigner` funcionan en Linux, macOS y Windows siempre que estén instalados con el `Android SDK`.