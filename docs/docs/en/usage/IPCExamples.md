# IPC Examples

Lambdaflow provides a simple RPC‐like IPC layer between your JavaScript frontend and C# backend. You “bind” a method name in C#, then call it from JS as a Promise that returns a JSON string containing either a `result` or an `error`.

---

## Sending messages from JS -> Backend

### JS side

To send a message from JavaScript to backend, you use the `send` method in JavaScript. This method is binded by default and writes the message in the backend standard input stream.

You can call it like this:
```javascript
send(message);
```
### Backend side (C# example)

To receive messages in C#, you need to read from the standard input, for example using `Console.ReadLine()` in C#. Check other languages examples to see how to do it in other languages.

```csharp
static void Main() {
    string line;
    while ((line = Console.ReadLine()) != null) {
        Console.WriteLine(line.ToUpperInvariant());
    }
}
```

This backend code will read input messages in a loop and print them in uppercase to the standard output.

## Sending messages from Backend -> JS

### Backend side (C# example)

The previous backend code is sending messages to the JS using `Console.WriteLine()`. This writes the message to the standard output, which is read by the JavaScript side. 

### JS side

To receive messages in JavaScript, you must define a `window.receive` function. This function will be called whenever a message is received from the backend.
```javascript
window.receive = function (response) {
    document.getElementById("out").textContent += response + "\n";
};
```

This function will be automatically called with the message that the backend writed in the standard output. You can then process the message as needed, for example, displaying it in a DOM element.