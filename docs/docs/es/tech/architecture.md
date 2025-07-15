# Visi�n General de la Arquitecturas

Visi�n general de la arquitectura de Lambdaflow, porqu� se eligi� esta arquitectura y c�mo se relacionan los componentes entre s�.

---

## 1. �Qu� es Lambdaflow y por qu� usarlo?

`Lambdaflow` nace de la necesidad de unir el poder del desarrollo web (HTML/CSS/JS) con cualquier backend (Python, Go, Rust, Java�), sin ataduras a NodeJS ni a servidores HTTP. Sus principales ventajas:

- `Flexibilidad total`: abstrae la comunicaci�n mediante un canal IPC ligero (stdin/stdout o named pipes).

- `Experiencia nativa`: integra el motor de navegador que ya dispone el sistema (WebView2, WebKitGTK, Android WebView).

- `Seguridad adaptativa`: desde cero comprobaciones (modo NONE) hasta firma y verificaci�n de recursos (modo INTEGRITY).

- `Ciclo de desarrollo �gil`: recarga instant�nea del frontend, logs detallados y pasos de build automatizados.

---

## 2. Componentes Principales

1. `AppLauncher`: punto de entrada y coordinador. Carga configuraci�n, inicializa seguridad, WebView e IPC.

2. `SecurityManager`: aplica la estrategia de seguridad elegida

	- NoSecurity (NONE) � Sin comprobaciones de seguridad. **Solo recomendado para aplicaciones que no requieran seguridad**.

	- DevSecurity (DEVELOPMENT) � Comprobaciones b�sicas para desarrollo. **Solo recomendado para desarrollo**.

	- IntegritySecurity (INTEGRITY) � Verifica la integridad de los recursos mediante firma digital. **Recomendado para producci�n**.


3. `IWebViewPlatform`: abstracci�n del control web en cada SO.

4. `IPCBridge` + `BackendProcessManager` + `MessageRouter`: maneja el ciclo de vida del proceso backend y el enrutamiento de mensajes JSON.

5. `IpcMessage`: modelo unificado para intercambio JSON.