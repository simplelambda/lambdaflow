# Documentación técnica

Si estás aquí, es porque quieres conocer como funciona LambdaFlow por dentro. En esta documentación podrás ver como funciona el
framework, sus componentes y que operaciones realiza en cada momento.

>**ATENCIÓN: Si estás buscando documentación sobre cómo usar LambdaFlow para crear aplicaciones, te recomendamos visitar la sección de
[`Documentación de uso`](../usage/index.md).**

---

## ¿Qué componentes principales tiene LambdaFlow?

- [`Compilación`](build/index.md) -> Scripts de Python que preparan el proyecto.
- [`Empaquetado`](packaging/index.md) -> Herramientas externas (NSIS en Windows, Makeself en Linux) que crean el instalador/paquete final para el usuario.
- [`Ejecución`](runtime/index.md) -> El Host en C# (el verdadero núcleo de LambdaFlow) que lanza backend + WebView y conecta todo.

LambdaFlow tiene como objetivo permitir el desarrollo de aplicaciones basadas en WebView desde cualquier SO, para cualquier SO
y sobre todo en cualquier lenguaje de programación para el backend. Es por eso que cada una de las partes del framework es fundamental.

---

## ¿En qué entorno se desarrolla con LambdaFlow?

Actualmente LambdaFlow únicamente está disponible para Windows, aunque se ampliará a Linux y Android. Además se recomienda utilizar
[`Visual Studio`](https://visualstudio.microsoft.com/) como entorno de desarrollo, ya que el diseño del framework está fuertemente ligado a este IDE, en el futuro se pretende
facilitar el uso de otros IDEs, pero por ahora es recomendable utilizar [`Visual Studio`](https://visualstudio.microsoft.com/).


---

## ¿Cómo comenzar?

Para empezar a comprender el funcionamiento de LambdaFlow, se recomienda comprender los componentes principales
mencionados anteriormente en el mismo orden en el que aparecen. Esto permitirá comprender cómo funciona el framework
y porqué se ha diseñado de la manera en que se ha hecho. Pulsando en los nombres de cada componente, te llevará a su respectiva
documentación.

---