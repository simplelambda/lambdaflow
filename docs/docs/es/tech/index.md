# Documentaci�n t�cnica

Si est�s aqu�, es porque quieres conocer como funciona LambdaFlow por dentro. En esta documentaci�n podr�s ver como funciona el
framework, sus componentes y que operaciones realiza en cada momento.

>**ATENCI�N: Si est�s buscando documentaci�n sobre c�mo usar LambdaFlow para crear aplicaciones, te recomendamos visitar la secci�n de
[`Documentaci�n de uso`](../usage/index.md).**

---

## �Qu� componentes principales tiene LambdaFlow?

- [`Compilaci�n`](build/index.md) -> Scripts de Python que preparan el proyecto.
- [`Empaquetado`](packaging/index.md) -> Herramientas externas (NSIS en Windows, Makeself en Linux) que crean el instalador/paquete final para el usuario.
- [`Ejecuci�n`](runtime/index.md) -> El Host en C# (el verdadero n�cleo de LambdaFlow) que lanza backend + WebView y conecta todo.

LambdaFlow tiene como objetivo permitir el desarrollo de aplicaciones basadas en WebView desde cualquier SO, para cualquier SO
y sobre todo en cualquier lenguaje de programaci�n para el backend. Es por eso que cada una de las partes del framework es fundamental.

---

## �En qu� entorno se desarrolla con LambdaFlow?

Actualmente LambdaFlow �nicamente est� disponible para Windows, aunque se ampliar� a Linux y Android. Adem�s se recomienda utilizar
[`Visual Studio`](https://visualstudio.microsoft.com/) como entorno de desarrollo, ya que el dise�o del framework est� fuertemente ligado a este IDE, en el futuro se pretende
facilitar el uso de otros IDEs, pero por ahora es recomendable utilizar [`Visual Studio`](https://visualstudio.microsoft.com/).


---

## �C�mo comenzar?

Para empezar a comprender el funcionamiento de LambdaFlow, se recomienda comprender los componentes principales
mencionados anteriormente en el mismo orden en el que aparecen. Esto permitir� comprender c�mo funciona el framework
y porqu� se ha dise�ado de la manera en que se ha hecho. Pulsando en los nombres de cada componente, te llevar� a su respectiva
documentaci�n.

---