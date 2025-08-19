# Documentación técnica

En esta sección se explica detalladamente como es el proceso de compilación de las aplicaciones creadas con LambdaFlow.

---

## ¿Qué modos de compilación existen?

LambdaFlow ofrece dos modos de compilación [`Build`]() y `Run`.

`Build` es el modo de compilación que prepara el proyecto para su producción, generando los archivos necesarios para empaquetar y ejecutar
la aplicación en los equipos de destino.

`Run` tiene como objetivo permitir al desarrollador de la aplicación ejecutar el proyecto de forma rápida y sencilla, sin necesidad de 
realizar procesos innecesarios para una prueba. Este modo es ideal para pruebas y desarrollo.

Se explicarán ambos modos, primeramente el modo `Build` ya que es el más complejo, después se pasará con el modo `Run`
que es más sencilla y una vez entendido el primer modo, el segundo no tiene nada nuevo.

---

## Launch settings

Antes de introducirse directamente a los dos modos de compilación y sus variantes, se recomienda ver la sección
de [`Launch settings`](launchSettings.md), ahí se explica como se configuran estos modos de compilación para que
Visual Studio permita ejecutarlos con el botón de "Play".

---