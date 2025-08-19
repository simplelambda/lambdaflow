# Documentaci�n t�cnica

En esta secci�n se explica detalladamente como es el proceso de compilaci�n de las aplicaciones creadas con LambdaFlow.

---

## �Qu� modos de compilaci�n existen?

LambdaFlow ofrece dos modos de compilaci�n [`Build`]() y `Run`.

`Build` es el modo de compilaci�n que prepara el proyecto para su producci�n, generando los archivos necesarios para empaquetar y ejecutar
la aplicaci�n en los equipos de destino.

`Run` tiene como objetivo permitir al desarrollador de la aplicaci�n ejecutar el proyecto de forma r�pida y sencilla, sin necesidad de 
realizar procesos innecesarios para una prueba. Este modo es ideal para pruebas y desarrollo.

Se explicar�n ambos modos, primeramente el modo `Build` ya que es el m�s complejo, despu�s se pasar� con el modo `Run`
que es m�s sencilla y una vez entendido el primer modo, el segundo no tiene nada nuevo.

---

## Launch settings

Antes de introducirse directamente a los dos modos de compilaci�n y sus variantes, se recomienda ver la secci�n
de [`Launch settings`](launchSettings.md), ah� se explica como se configuran estos modos de compilaci�n para que
Visual Studio permita ejecutarlos con el bot�n de "Play".

---