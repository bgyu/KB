# GRPC Introduction

gRPC (Google Remote Procedure Call) is a high-performance, open-source RPC (Remote Procedure Call) framework that enables communication between distributed systems. It is widely used for microservices architectures, cloud applications, and APIs. Here are its **core concepts**:

---

### 1️⃣ **Protocol Buffers (protobuf)**
- gRPC uses **Protocol Buffers (protobufs)** as its interface definition language (IDL).
- It defines the structure of requests and responses in a compact and efficient binary format.
- Example `.proto` file:
  ```proto
  syntax = "proto3";

  service Greeter {
    rpc SayHello (HelloRequest) returns (HelloResponse);
  }

  message HelloRequest {
    string name = 1;
  }

  message HelloResponse {
    string message = 1;
  }
  ```

---

### 2️⃣ **Service Definition**
- Services are defined in `.proto` files and describe **RPC methods** that clients can call.
- A service consists of **methods** (like functions) that accept requests and return responses.

---

### 3️⃣ **gRPC Communication Types**
gRPC supports different communication patterns:

| **Type**               | **Client Sends** | **Server Sends** | **Use Case** |
|------------------------|-----------------|-----------------|--------------|
| **Unary RPC**          | 1 request       | 1 response      | Simple request-response |
| **Server Streaming**   | 1 request       | Stream of responses | Data feeds, logs |
| **Client Streaming**   | Stream of requests | 1 response | Uploading large files |
| **Bidirectional Streaming** | Stream of requests | Stream of responses | Chat apps, video calls |

---

### 4️⃣ **gRPC Client & Server**
- **Client**: Calls the RPC methods and sends requests.
- **Server**: Implements the service methods and returns responses.
- Example:
  - **Server in Python**
    ```python
    import grpc
    from concurrent import futures
    import greeter_pb2
    import greeter_pb2_grpc

    class GreeterServicer(greeter_pb2_grpc.GreeterServicer):
        def SayHello(self, request, context):
            return greeter_pb2.HelloResponse(message=f"Hello, {request.name}")

    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    greeter_pb2_grpc.add_GreeterServicer_to_server(GreeterServicer(), server)
    server.add_insecure_port("[::]:50051")
    server.start()
    server.wait_for_termination()
    ```

  - **Client in Python**
    ```python
    import grpc
    import greeter_pb2
    import greeter_pb2_grpc

    channel = grpc.insecure_channel("localhost:50051")
    stub = greeter_pb2_grpc.GreeterStub(channel)
    response = stub.SayHello(greeter_pb2.HelloRequest(name="Alice"))
    print(response.message)
    ```

---

### 5️⃣ **gRPC Features**
- **Language Agnostic**: Supports multiple languages (Python, Go, Java, C++, etc.).
- **Efficient & Lightweight**: Uses **HTTP/2** for multiplexed streaming.
- **Streaming Support**: Supports both client and server streaming.
- **Authentication & Security**: Supports **TLS encryption** and **authentication**.
- **Interoperability**: Can be used with **REST gateways**.

---

### 6️⃣ **gRPC vs REST**
| Feature | gRPC | REST |
|---------|------|------|
| **Protocol** | HTTP/2 | HTTP/1.1 |
| **Data Format** | Protobuf (binary) | JSON (text-based) |
| **Performance** | Faster | Slower |
| **Streaming** | Yes (full-duplex) | Limited |
| **Type Safety** | Strong (Protobuf) | Weak (JSON) |

---

### 🔥 **When to Use gRPC?**
✅ **Microservices Communication**  
✅ **High-performance APIs**  
✅ **Streaming & Real-time Communication**  
✅ **Multi-language Support**  

❌ **When Not to Use?**  
- If **JSON/REST** is required for compatibility.
- When **browser-based** clients are the primary consumers (gRPC requires gRPC-Web for browsers).

---

### 🎯 **Conclusion**
gRPC is a powerful alternative to REST, offering **speed, efficiency, and streaming capabilities**. It's widely used in **cloud-native applications, microservices, and inter-service communication**.


# GRPC Example (C++)
Here's a **gRPC example in C++** implementing a simple **Greeter** service where a client sends a name, and the server responds with a greeting.

---

## **1️⃣ Define the gRPC Service in a `.proto` file**
Create a file named **`greeter.proto`**:

```proto
syntax = "proto3";

package greeter;

// Define the service
service Greeter {
  rpc SayHello (HelloRequest) returns (HelloResponse);
}

// Define request and response messages
message HelloRequest {
  string name = 1;
}

message HelloResponse {
  string message = 1;
}
```

Run the **Protocol Buffer Compiler (protoc)** to generate C++ code:
```sh
protoc -I=. --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` greeter.proto
protoc -I=. --cpp_out=. greeter.proto
```
This generates:
- `greeter.grpc.pb.h` (gRPC-specific code)
- `greeter.pb.h` and `greeter.pb.cc` (Protocol Buffers message definitions)

---

## **2️⃣ Implement the gRPC Server in C++**
Create a file named **`greeter_server.cpp`**:

```cpp
#include <iostream>
#include <memory>
#include <string>

#include <grpcpp/grpcpp.h>
#include "greeter.grpc.pb.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using greeter::Greeter;
using greeter::HelloRequest;
using greeter::HelloResponse;

// Implement the service
class GreeterServiceImpl final : public Greeter::Service {
public:
    Status SayHello(ServerContext* context, const HelloRequest* request, HelloResponse* reply) override {
        std::string message = "Hello, " + request->name();
        reply->set_message(message);
        std::cout << "Received request from: " << request->name() << std::endl;
        return Status::OK;
    }
};

void RunServer() {
    std::string server_address("0.0.0.0:50051");
    GreeterServiceImpl service;

    ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(&service);

    std::unique_ptr<Server> server(builder.BuildAndStart());
    std::cout << "Server listening on " << server_address << std::endl;
    server->Wait();
}

int main() {
    RunServer();
    return 0;
}
```

Compile the server:
```sh
g++ -std=c++17 greeter_server.cpp greeter.pb.cc greeter.grpc.pb.cc -o server `pkg-config --cflags --libs grpc++ protobuf`
```

Run the server:
```sh
./server
```

---

## **3️⃣ Implement the gRPC Client in C++**
Create a file named **`greeter_client.cpp`**:

```cpp
#include <iostream>
#include <memory>
#include <string>

#include <grpcpp/grpcpp.h>
#include "greeter.grpc.pb.h"

using grpc::Channel;
using grpc::ClientContext;
using grpc::Status;
using greeter::Greeter;
using greeter::HelloRequest;
using greeter::HelloResponse;

// Client class to communicate with the gRPC server
class GreeterClient {
public:
    GreeterClient(std::shared_ptr<Channel> channel) : stub_(Greeter::NewStub(channel)) {}

    std::string SayHello(const std::string& name) {
        HelloRequest request;
        request.set_name(name);

        HelloResponse reply;
        ClientContext context;

        Status status = stub_->SayHello(&context, request, &reply);
        if (status.ok()) {
            return reply.message();
        } else {
            return "RPC failed: " + status.error_message();
        }
    }

private:
    std::unique_ptr<Greeter::Stub> stub_;
};

int main() {
    GreeterClient client(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials()));

    std::string user_name;
    std::cout << "Enter your name: ";
    std::cin >> user_name;

    std::string response = client.SayHello(user_name);
    std::cout << "Server Response: " << response << std::endl;

    return 0;
}
```

Compile the client:
```sh
g++ -std=c++17 greeter_client.cpp greeter.pb.cc greeter.grpc.pb.cc -o client `pkg-config --cflags --libs grpc++ protobuf`
```

Run the client:
```sh
./client
```

---

## **4️⃣ Expected Output**
### **Server Terminal:**
```sh
Server listening on 0.0.0.0:50051
Received request from: Alice
```

### **Client Terminal:**
```sh
Enter your name: Alice
Server Response: Hello, Alice
```

---

## **5️⃣ Summary**
- Defined the **gRPC service** in a `.proto` file.
- Implemented a **gRPC server** in C++ that listens on port **50051**.
- Implemented a **gRPC client** in C++ that calls the `SayHello` RPC.
- Used **Protocol Buffers** for efficient serialization.
- Built the project using **g++ with gRPC and Protobuf**.


# GRPC Client Stub and Why
### **What is a Stub in gRPC?**
A **stub** in gRPC is a **client-side proxy** that provides a simple way for the client to invoke remote procedure calls (RPCs) on the server.

When you define a gRPC service using **Protocol Buffers (`.proto` file)**, the gRPC framework **automatically generates** the client-side stub, which abstracts the complexity of making network calls.

---

### **Stub in the C++ gRPC Client**
In the C++ client example, the stub is created in this line:

```cpp
std::unique_ptr<Greeter::Stub> stub_;
```

- `Greeter::Stub` is the **auto-generated client stub** class.
- It provides methods that allow the client to call the gRPC server.

### **How the Stub Works**
1. **Create a gRPC channel** (a connection to the server):
   ```cpp
   std::shared_ptr<Channel> channel = grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials());
   ```
   - This creates a connection to the **server running on localhost:50051**.
   - `InsecureChannelCredentials()` means we're not using SSL/TLS encryption (for simplicity).

2. **Instantiate the Stub**
   ```cpp
   std::unique_ptr<Greeter::Stub> stub_ = Greeter::NewStub(channel);
   ```
   - `NewStub(channel)` creates a **proxy object** for calling remote methods on the server.
   - This stub handles **serialization, deserialization, and network communication** for the client.

3. **Use the Stub to Make an RPC Call**
   ```cpp
   Status status = stub_->SayHello(&context, request, &reply);
   ```
   - The client **calls the `SayHello` method** on the stub.
   - The stub:
     - Serializes the request (`HelloRequest`).
     - Sends the request over the network.
     - Waits for a response from the server.
     - Deserializes the response (`HelloResponse`).
     - Returns the result to the client.

---

### **Stub in Other Languages**
- **Python:** `stub = GreeterStub(channel)`
- **Go:** `client := pb.NewGreeterClient(conn)`
- **Java:** `GreeterGrpc.newBlockingStub(channel)`

Each language's stub **automatically** handles:
- **Network communication**
- **Marshalling/unmarshalling of Protocol Buffers**
- **Retry mechanisms (if enabled)**

---

### **Why Use a Stub?**
✅ **Simplifies client-side code** – No need to manually handle network communication.  
✅ **Encapsulates serialization and deserialization** – You just call functions.  
✅ **Ensures type safety** – Methods use the types defined in `.proto`.  
✅ **Works with gRPC features** – Supports authentication, streaming, etc.  

---

### **Conclusion**
A **stub** in gRPC acts as a **client-side proxy** that allows applications to call remote methods as if they were local functions. It abstracts away networking complexities, making **inter-service communication** easy and efficient.


# How GRPC Stub Works
Here’s a **visual breakdown** of how a **gRPC stub** works, along with an **illustrative diagram**.

---

## **🔍 How a gRPC Stub Works**
### **Step-by-Step Flow**
1. **Client Calls the Stub Method**  
   The client code calls a function on the **stub** (e.g., `SayHello`).
   
2. **Stub Packs the Request**  
   - The stub **serializes** (encodes) the request into **protobuf format**.
   - It prepares to send the data over the network.

3. **Stub Sends the Request Over gRPC Channel**  
   - The stub uses **gRPC runtime** to send the request over **HTTP/2**.
   - If using **TLS**, the data is encrypted before transmission.

4. **Server Receives the Request & Processes It**  
   - The **gRPC server handler** receives the request.
   - It **deserializes** (decodes) the protobuf message.
   - The server executes the **corresponding function** (e.g., `SayHello`).

5. **Server Sends Back the Response**  
   - The server **serializes** (encodes) the response.
   - It transmits it back to the client over the **gRPC channel**.

6. **Stub Receives & Unpacks the Response**  
   - The stub **decodes** the protobuf response.
   - It returns the data as a native object to the client.

7. **Client Uses the Response**  
   - The client **gets the response** and proceeds with further operations.

---

## **🎨 Visual Diagram**

```plaintext
           CLIENT SIDE                            SERVER SIDE
+---------------------------------+      +----------------------------------+
| Application Code                |      | Application Code                 |
|  - GreeterClient                |      |  - GreeterServiceImpl            |
|  - Calls stub_->SayHello()       |      |  - Implements SayHello()         |
+---------------------------------+      +----------------------------------+
          |  (Call SayHello)                        |  (Process request)
          v                                        ^
+---------------------------------+      +----------------------------------+
| gRPC Stub (Client Proxy)        |      | gRPC Server Handler             |
|  - Serializes Request           |      |  - Deserializes Request         |
|  - Sends Request via HTTP/2      | ----> |  - Calls actual function       |
|  - Waits for Response            | <---- |  - Serializes Response        |
|  - Deserializes Response         |      |  - Sends Response via HTTP/2   |
+---------------------------------+      +----------------------------------+
          |                                        ^
          v                                        |
+---------------------------------+      +----------------------------------+
| gRPC Runtime & Network          |      | gRPC Runtime & Network           |
|  - Handles Connection           | ----> |  - Handles Connection           |
|  - Manages HTTP/2 Streams       | <---- |  - Manages HTTP/2 Streams      |
|  - Implements Load Balancing    |      |  - Implements Load Balancing    |
+---------------------------------+      +----------------------------------+

```

---

## **🛠 Example in C++ (How Stub Works in Code)**

### **Client Side**
```cpp
// 1️⃣ Create a gRPC channel to connect to the server
std::shared_ptr<Channel> channel = grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials());

// 2️⃣ Create the stub (proxy)
std::unique_ptr<Greeter::Stub> stub_ = Greeter::NewStub(channel);

// 3️⃣ Prepare request
HelloRequest request;
request.set_name("Alice");

// 4️⃣ Create a response object
HelloResponse reply;

// 5️⃣ Set up the RPC call context
ClientContext context;

// 6️⃣ Make the RPC call via stub
Status status = stub_->SayHello(&context, request, &reply);

// 7️⃣ Process the response
if (status.ok()) {
    std::cout << "Server Response: " << reply.message() << std::endl;
} else {
    std::cout << "RPC failed: " << status.error_message() << std::endl;
}
```

---

## **📝 Key Takeaways**
✅ **The stub acts as a client-side proxy** – It hides the networking complexity.  
✅ **Automatically handles serialization and deserialization** – Uses **protobuf** for efficient encoding/decoding.  
✅ **Uses HTTP/2 for efficient communication** – Supports multiplexed requests.  
✅ **Ensures type safety** – Only allows properly defined requests and responses.  
✅ **Stub makes calling a remote function feel like a local function call** – No need to manually handle network sockets.

---

# Grp Channel
### **How Many Channels Does a gRPC Client Need?**
A gRPC client typically **needs only one channel** to communicate with a gRPC server. However, the number of channels required depends on **concurrency needs, performance goals, and connection management**.

---

## **1️⃣ What is a gRPC Channel?**
A **gRPC channel** is a virtual connection between the client and the server over **HTTP/2**.

- The client **creates a channel** to the server and uses it to send **multiple requests**.
- The channel **handles connection management, load balancing, retries, and multiplexing**.
- A **single channel can support multiple RPC calls** simultaneously due to HTTP/2 **multiplexing**.

### **Example of Creating One gRPC Channel (C++ Client)**
```cpp
std::shared_ptr<grpc::Channel> channel = grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials());
std::unique_ptr<Greeter::Stub> stub = Greeter::NewStub(channel);
```
- **Only one channel is created**.
- The client can make multiple RPC calls over this channel.

---

## **2️⃣ When to Use Multiple Channels?**
In most cases, **one channel is sufficient**. However, multiple channels may be needed in the following situations:

### **🟢 Case 1: Connecting to Multiple gRPC Services**
If a client communicates with **different gRPC servers**, separate channels are required for each.

```cpp
auto channel1 = grpc::CreateChannel("server1:50051", grpc::InsecureChannelCredentials());
auto channel2 = grpc::CreateChannel("server2:50052", grpc::InsecureChannelCredentials());
```

### **🟢 Case 2: Load Balancing Across Multiple Server Instances**
gRPC supports **client-side load balancing**. A single channel can connect to **multiple backend servers** using **round-robin or other strategies**.

```cpp
auto channel = grpc::CreateChannel("server1:50051,server2:50051", grpc::InsecureChannelCredentials());
```

### **🟢 Case 3: High Concurrency & Performance Optimization**
- A single channel is **thread-safe** but may become a bottleneck under **heavy load**.
- If **thousands of requests per second** are needed, multiple channels can help **distribute the load**.

```cpp
std::vector<std::shared_ptr<grpc::Channel>> channels;
for (int i = 0; i < 5; i++) {
    channels.push_back(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials()));
}
```

---

## **3️⃣ Rule of Thumb for Choosing Number of Channels**
| **Scenario** | **Recommended Channels** |
|-------------|--------------------|
| Simple gRPC client making occasional calls | **1 channel** |
| High-load client with heavy traffic (many parallel RPCs) | **Multiple channels** |
| Connecting to different servers/services | **1 per server** |
| Using **client-side load balancing** | **1 channel (gRPC handles load balancing internally)** |

---

## **🔍 Conclusion**
✅ **One channel is sufficient in most cases** – HTTP/2 allows multiple RPCs over a single connection.  
✅ **Multiple channels may help in high-load scenarios** – When one channel is a bottleneck.  
✅ **Different services need different channels** – If connecting to multiple gRPC servers.  
✅ **Client-side load balancing can work with a single channel** – gRPC can distribute traffic across multiple backend servers.


# Protobuf basic
### **Protocol Buffers (Protobuf) Syntax and Core Concepts**  
Protobuf (Protocol Buffers) is a **language-neutral, platform-neutral** format for defining structured data. It is used extensively in **gRPC** for serialization.

---

## **1️⃣ Basic Syntax: `proto3`**
The latest version of Protobuf is **proto3**, which simplifies message definitions.

### **Basic Example: `greeter.proto`**
```proto
syntax = "proto3";  // Define protobuf version

package greeter;    // Organize related messages and services

// Define a service (for gRPC)
service Greeter {
  rpc SayHello (HelloRequest) returns (HelloResponse);
}

// Define request message
message HelloRequest {
  string name = 1;
}

// Define response message
message HelloResponse {
  string message = 1;
}
```

---

## **2️⃣ Core Concepts of Protobuf**
### **📌 1. Syntax Version**
- `proto3` (latest) is recommended.
- `proto2` is older but has more manual control.

```proto
syntax = "proto3";  // Required in proto3 files
```

---

### **📌 2. Packages**
- Avoids naming conflicts in large projects.
- Works like namespaces in C++ or Java.

```proto
package myapp;
```

---

### **📌 3. Messages (Data Structures)**
- Messages define structured data, like JSON objects.
- Each field has a **name**, a **type**, and a **unique number**.

```proto
message Person {
  string name = 1;
  int32 age = 2;
  bool is_student = 3;
}
```
💡 **Why numbers?** They determine the binary field position for efficient serialization.

---

### **📌 4. Data Types**
Protobuf supports **scalars, enums, and complex types**.

| **Type**   | **Description** |
|------------|----------------|
| `string`   | Text data |
| `int32`    | 32-bit integer |
| `int64`    | 64-bit integer |
| `bool`     | Boolean (`true/false`) |
| `float`    | 32-bit floating point |
| `double`   | 64-bit floating point |
| `bytes`    | Raw binary data |
| `enum`     | Enumeration |

#### **Example: Scalars, Enums, and Nested Messages**
```proto
enum Gender {
  UNKNOWN = 0;
  MALE = 1;
  FEMALE = 2;
}

message Student {
  string name = 1;
  int32 age = 2;
  Gender gender = 3;
  Address home_address = 4; // Nested message
}

message Address {
  string city = 1;
  string country = 2;
}
```

---

### **📌 5. Repeated Fields (Lists)**
- To store multiple values of the same type (like an array or list).

```proto
message Classroom {
  string name = 1;
  repeated Student students = 2;  // List of students
}
```
💡 **`repeated` means an array of `Student` objects.**

---

### **📌 6. Optional Fields (proto3)**
- In `proto3`, fields are always optional by default.
- If a field is **not set**, it takes the **default value**.

```proto
message Car {
  string model = 1;  // Default: ""
  int32 year = 2;    // Default: 0
}
```
- `model = ""` if unset
- `year = 0` if unset

✅ **Use `optional` keyword for explicit control (since Protobuf v3.15)**:
```proto
message Car {
  optional string model = 1;  // Tracks if field was set
}
```

---

### **📌 7. Reserved Fields**
- **Prevents conflicts** if the schema changes.

```proto
message OldMessage {
  reserved 2, 3, 5 to 10;  // Prevents reuse
  reserved "old_field";    // Prevents old name reuse
}
```

---

### **📌 8. Services (For gRPC)**
- Defines **remote procedure calls (RPCs)**.

```proto
service Calculator {
  rpc Add (AddRequest) returns (AddResponse);
}

message AddRequest {
  int32 a = 1;
  int32 b = 2;
}

message AddResponse {
  int32 sum = 1;
}
```

---

## **3️⃣ Generating Code from Protobuf**
Use `protoc` (Protocol Buffer Compiler) to generate language-specific code.

### **🔹 Generate C++ Code**
```sh
protoc --cpp_out=. calculator.proto
```

### **🔹 Generate Python Code**
```sh
protoc --python_out=. calculator.proto
```

### **🔹 Generate gRPC Stubs (For gRPC Servers & Clients)**
```sh
protoc --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` calculator.proto
```

---

## **4️⃣ Protobuf vs JSON**
| Feature        | Protobuf 🚀   | JSON 📜  |
|---------------|-------------|---------|
| **Size**      | Smaller (binary) | Larger (text) |
| **Speed**     | Faster (binary)  | Slower (text-based) |
| **Readability** | Not human-readable | Human-readable |
| **Backward Compatibility** | Yes | No (schema-less) |
| **Performance** | High | Medium |
| **Usage** | gRPC, APIs | REST, Config files |

---

## **🔍 Summary**
✅ **Protobuf is compact and efficient** (binary format).  
✅ **It supports strong typing** (avoids errors).  
✅ **Works across languages** (C++, Python, Go, Java, etc.).  
✅ **Best for high-performance APIs** (e.g., gRPC).  

# Relationship Between Protobuf and Generated C++ Code
### **Relationship Between Protobuf and Generated C++ Code**
When you define a **`.proto`** file using Protocol Buffers (Protobuf), the **Protocol Buffer Compiler (`protoc`)** generates C++ classes that allow you to **serialize, deserialize, and manipulate** structured data efficiently.

---

## **🔍 Step-by-Step Process**
### **1️⃣ Define a `.proto` File**
You start by defining a **`.proto`** file that describes the data structure.

#### **Example: `person.proto`**
```proto
syntax = "proto3";

package mypackage;

// Define a message (data structure)
message Person {
  string name = 1;
  int32 age = 2;
}
```

---

### **2️⃣ Compile the Protobuf File to C++ Code**
Run the **Protocol Buffer Compiler (`protoc`)** to generate the corresponding C++ classes:

```sh
protoc --cpp_out=. person.proto
```
This command generates:
- **`person.pb.h`** (Header file with class definitions)
- **`person.pb.cc`** (Implementation file)

---

### **3️⃣ What’s Inside the Generated C++ Code?**
#### **📌 (A) The Header File (`person.pb.h`)**
This file contains **C++ class definitions** corresponding to the `.proto` file.

```cpp
// Generated C++ class for Person
class Person : public ::google::protobuf::Message {
public:
    Person();
    ~Person();

    // Getters & Setters
    void set_name(const std::string& name);
    std::string name() const;

    void set_age(int32_t age);
    int32_t age() const;

    // Serialization & Parsing
    std::string SerializeAsString() const;
    bool ParseFromString(const std::string& data);
};
```
💡 **Key Features in `person.pb.h`:**
- **Getter & Setter Methods** (`set_name()`, `name()`)
- **Serialization Methods** (`SerializeAsString()`)
- **Deserialization Methods** (`ParseFromString()`)

---

#### **📌 (B) The Implementation File (`person.pb.cc`)**
This file contains the **implementation** of the `Person` class methods.

Example:
```cpp
void Person::set_name(const std::string& name) {
    name_ = name;
}

std::string Person::name() const {
    return name_;
}

void Person::set_age(int32_t age) {
    age_ = age;
}

int32_t Person::age() const {
    return age_;
}
```
---

### **4️⃣ Using the Generated C++ Code**
Now, you can use the `Person` class in your C++ program.

#### **Example: Serialize & Deserialize a `Person` Object**
```cpp
#include <iostream>
#include "person.pb.h"

int main() {
    // Create a Person object
    Person person;
    person.set_name("Alice");
    person.set_age(25);

    // Serialize to a string
    std::string serialized_data;
    person.SerializeToString(&serialized_data);

    std::cout << "Serialized Data: " << serialized_data << std::endl;

    // Deserialize back to a Person object
    Person new_person;
    if (new_person.ParseFromString(serialized_data)) {
        std::cout << "Deserialized Name: " << new_person.name() << std::endl;
        std::cout << "Deserialized Age: " << new_person.age() << std::endl;
    }

    return 0;
}
```

#### **Output**
```sh
Serialized Data: (Binary format)
Deserialized Name: Alice
Deserialized Age: 25
```

---

## **🔍 Summary: Protobuf & C++ Code Relationship**
| **Step** | **Description** |
|----------|---------------|
| **Define `.proto` file** | Defines data structures & services |
| **Run `protoc` compiler** | Generates C++ classes (`.pb.h` & `.pb.cc`) |
| **Use Generated C++ Code** | Call getters, setters, serialization & deserialization methods |

✅ **Protobuf automatically generates efficient C++ code for structured data handling.**  
✅ **The generated classes provide easy-to-use functions for serialization & deserialization.**  
✅ **You just write `.proto` files, and `protoc` generates everything else!** 🚀  


# Full Example: Protobuf with gRPC in C++
### **Full Example: Protobuf with gRPC in C++**
This example demonstrates **how to use Protobuf with gRPC in C++** by implementing a **Greeter service** that sends a greeting message.

---

## **1️⃣ Define the Protobuf File (`greeter.proto`)**
Create a file named **`greeter.proto`**, which defines:
- A **gRPC service** (`Greeter`).
- A **request message** (`HelloRequest`).
- A **response message** (`HelloResponse`).

```proto
syntax = "proto3";

package greeter;

// Define the gRPC service
service Greeter {
  rpc SayHello (HelloRequest) returns (HelloResponse);
}

// Define the request message
message HelloRequest {
  string name = 1;
}

// Define the response message
message HelloResponse {
  string message = 1;
}
```

---

## **2️⃣ Generate C++ Code from Protobuf**
Run the following **`protoc`** commands to generate **C++ files**:

```sh
protoc --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_cpp_plugin` greeter.proto
protoc --cpp_out=. greeter.proto
```

This will generate:
- `greeter.pb.h` & `greeter.pb.cc` → **Protobuf message classes**.
- `greeter.grpc.pb.h` & `greeter.grpc.pb.cc` → **gRPC service stub classes**.

---

## **3️⃣ Implement the gRPC Server (`greeter_server.cpp`)**
Now, implement the **gRPC server** that listens for requests and responds with a greeting.

```cpp
#include <iostream>
#include <memory>
#include <string>
#include <grpcpp/grpcpp.h>
#include "greeter.grpc.pb.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using greeter::Greeter;
using greeter::HelloRequest;
using greeter::HelloResponse;

// Implement the Greeter service
class GreeterServiceImpl final : public Greeter::Service {
public:
    Status SayHello(ServerContext* context, const HelloRequest* request, HelloResponse* reply) override {
        std::string message = "Hello, " + request->name();
        reply->set_message(message);
        std::cout << "Received request from: " << request->name() << std::endl;
        return Status::OK;
    }
};

void RunServer() {
    std::string server_address("0.0.0.0:50051");
    GreeterServiceImpl service;

    ServerBuilder builder;
    builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
    builder.RegisterService(&service);

    std::unique_ptr<Server> server(builder.BuildAndStart());
    std::cout << "Server listening on " << server_address << std::endl;
    server->Wait();
}

int main() {
    RunServer();
    return 0;
}
```

### **🛠 Compile the Server**
```sh
g++ -std=c++17 greeter_server.cpp greeter.pb.cc greeter.grpc.pb.cc -o server `pkg-config --cflags --libs grpc++ protobuf`
```

### **🚀 Run the Server**
```sh
./server
```

**Expected Output:**
```sh
Server listening on 0.0.0.0:50051
```

---

## **4️⃣ Implement the gRPC Client (`greeter_client.cpp`)**
Now, implement the **gRPC client** that sends a name and receives a greeting from the server.

```cpp
#include <iostream>
#include <memory>
#include <string>
#include <grpcpp/grpcpp.h>
#include "greeter.grpc.pb.h"

using grpc::Channel;
using grpc::ClientContext;
using grpc::Status;
using greeter::Greeter;
using greeter::HelloRequest;
using greeter::HelloResponse;

// Client class to communicate with the server
class GreeterClient {
public:
    GreeterClient(std::shared_ptr<Channel> channel) : stub_(Greeter::NewStub(channel)) {}

    std::string SayHello(const std::string& name) {
        HelloRequest request;
        request.set_name(name);

        HelloResponse reply;
        ClientContext context;

        Status status = stub_->SayHello(&context, request, &reply);
        if (status.ok()) {
            return reply.message();
        } else {
            return "RPC failed: " + status.error_message();
        }
    }

private:
    std::unique_ptr<Greeter::Stub> stub_;
};

int main() {
    GreeterClient client(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials()));

    std::string user_name;
    std::cout << "Enter your name: ";
    std::cin >> user_name;

    std::string response = client.SayHello(user_name);
    std::cout << "Server Response: " << response << std::endl;

    return 0;
}
```

### **🛠 Compile the Client**
```sh
g++ -std=c++17 greeter_client.cpp greeter.pb.cc greeter.grpc.pb.cc -o client `pkg-config --cflags --libs grpc++ protobuf`
```

### **🚀 Run the Client**
```sh
./client
```

### **📝 Example Interaction**
#### **Server Output**
```sh
Server listening on 0.0.0.0:50051
Received request from: Alice
```

#### **Client Output**
```sh
Enter your name: Alice
Server Response: Hello, Alice
```

---

## **5️⃣ Understanding the Relationship Between Protobuf and gRPC**
| **Component** | **Description** |
|--------------|----------------|
| **Protobuf (`greeter.proto`)** | Defines the data structure and service API |
| **Generated C++ Files** | `protoc` creates `greeter.pb.h`, `greeter.pb.cc`, `greeter.grpc.pb.h`, and `greeter.grpc.pb.cc` |
| **gRPC Server (`greeter_server.cpp`)** | Implements the service and listens for requests |
| **gRPC Client (`greeter_client.cpp`)** | Calls the service using the generated **stub** |

---

## **🔍 Summary**
✅ **Defined Protobuf messages and gRPC service** (`greeter.proto`).  
✅ **Generated C++ code** using `protoc`.  
✅ **Implemented the gRPC Server** (`greeter_server.cpp`).  
✅ **Implemented the gRPC Client** (`greeter_client.cpp`).  
✅ **Compiled and ran both the server and client successfully!** 🚀  


# Requirements for Field Numbers in Protobuf
### **Requirements for Field Numbers in Protobuf**
Field numbers in Protobuf are essential for **serialization and deserialization**. They uniquely identify each field in a message. Here are the rules and best practices:

---

## **1️⃣ Valid Range of Field Numbers**
| **Range** | **Usage** |
|-----------|----------|
| **1 – 15** | Uses **1-byte encoding** (efficient for frequently used fields) |
| **16 – 2047** | Uses **2-byte encoding** |
| **19000 – 19999** | **Reserved by Protobuf** (cannot be used) |
| **≥ 2,048** | Takes more space (3 bytes or more) |

🚫 **Invalid values:**  
- **0 is not allowed** (Protobuf reserves `0` for internal use).
- **Negative numbers are not allowed**.

---

## **2️⃣ Do Field Numbers Need to Be in Order?**
❌ **No, field numbers do not need to be sequential.**  
✅ **They must be unique within a message.**  

### **Valid Example (Non-Sequential)**
```proto
message User {
  string name = 3;     // OK
  int32 age = 10;      // OK (not in order)
  bool is_active = 1;  // OK (lower number)
}
```

### **Invalid Example (Duplicate Field Numbers)**
```proto
message User {
  string name = 1;
  int32 age = 1;  // ❌ ERROR: Duplicate field number
}
```

---

## **3️⃣ Why Are Field Numbers Important?**
Field numbers are crucial because **Protobuf uses them for encoding and decoding messages**.

- If you **change a field number**, older clients/servers **will not recognize** the new data.
- **Adding new fields is safe** as long as old numbers are not reused.

---

## **4️⃣ Reserved Field Numbers**
If you plan to **remove a field**, you should **reserve its field number** to prevent accidental reuse.

```proto
message User {
  reserved 2, 3, 5 to 10;  // Prevents reuse of these numbers
}
```

---

## **🔍 Summary**
✅ **Field numbers must be unique within a message.**  
✅ **Valid range: `1 – 2047` (efficient), `2048+` (less efficient), `19000 – 19999` (reserved).**  
✅ **Field number `0` and negative numbers are not allowed.**  
✅ **Field numbers don't need to be sequential but must be unique.**  
✅ **Reserve field numbers before removing a field to avoid breaking compatibility.**  

# Handling Backward Compatibility in Protobuf
### **Handling Backward Compatibility in Protobuf**
Backward compatibility in Protobuf ensures that **newer versions of a message** can work with **older versions** without breaking existing functionality.

---

## **1️⃣ Safe Changes (Backward Compatible ✅)**
These changes **won't break** older versions of your application:

### **✅ Adding New Fields**
- **New fields should use new field numbers**.
- **Older clients ignore unknown fields** safely.

```proto
// Old version
message User {
  string name = 1;
  int32 age = 2;
}

// New version (adding a field is safe!)
message User {
  string name = 1;
  int32 age = 2;
  string email = 3;  // ✅ Safe: Older clients just ignore this field
}
```

---

### **✅ Removing Fields (Reserve the Field Number)**
- Instead of deleting a field, **use the `reserved` keyword** to prevent future conflicts.

```proto
message User {
  reserved 2;          // Prevents reuse of field number 2
  reserved "age";      // Prevents reuse of field name "age"
  string name = 1;
}
```

---

### **✅ Changing Field Names (If the Number Stays the Same)**
- The field **number** matters, not the name.

```proto
// Old version
message User {
  string username = 1;
}

// New version (renaming is safe)
message User {
  string name = 1;  // ✅ Safe: Still using field number 1
}
```

---

## **2️⃣ Breaking Changes (Not Backward Compatible ❌)**
These changes will **break compatibility** with older clients:

### **❌ Changing a Field Number**
If you change a field’s number, **existing serialized data becomes unreadable**.

```proto
// Old version
message User {
  string name = 1;
}

// New version (BAD: changing field number breaks decoding!)
message User {
  string name = 2;  // ❌ Breaks old clients
}
```

---

### **❌ Changing a Field Type**
Changing a field’s type **causes decoding failures**.

```proto
// Old version
message User {
  int32 age = 1;
}

// New version (BAD: Changing type breaks compatibility!)
message User {
  string age = 1;  // ❌ Old clients expect an integer, not a string!
}
```

---

### **❌ Changing `repeated` to Non-repeated or Vice Versa**
Switching between **single-value fields** and **lists** breaks decoding.

```proto
// Old version
message User {
  repeated string hobbies = 1;
}

// New version (BAD: Changing repeated to single-value breaks compatibility!)
message User {
  string hobbies = 1;  // ❌ Old clients expect a list, not a string!
}
```

---

### **❌ Changing `oneof` Fields**
- **Oneof fields are mutually exclusive**.
- Changing existing fields inside a `oneof` **breaks compatibility**.

```proto
// Old version
message User {
  oneof contact {
    string email = 1;
    string phone = 2;
  }
}

// New version (BAD: Changing oneof fields breaks compatibility!)
message User {
  oneof contact {
    string email = 1;
    int32 phone = 2;  // ❌ BAD: Old clients expect a string, not an int!
  }
}
```

---

## **3️⃣ Best Practices for Backward Compatibility**
✅ **Use unique field numbers** (never reuse old numbers).  
✅ **Only add new fields** (don’t modify existing ones).  
✅ **Reserve field numbers before removing them** to avoid conflicts.  
✅ **Use default values carefully** (e.g., `0` for integers, `""` for strings).  
✅ **Use `oneof` carefully**, as changes can break old clients.  

---

### **🔍 Example: Evolving a Message Without Breaking Compatibility**
1️⃣ **Initial Version**  
```proto
message User {
  string name = 1;
  int32 age = 2;
}
```

2️⃣ **New Version (Compatible)**  
```proto
message User {
  string name = 1;
  reserved 2;       // Prevents reuse of old "age" field
  reserved "age";  
  string email = 3;  // ✅ Added safely
}
```

---

## **🎯 Summary**
| **Change** | **Backward Compatible?** |
|------------|-------------------------|
| **Adding new fields** | ✅ Yes |
| **Renaming a field** | ✅ Yes (if number stays the same) |
| **Reserving a field number** | ✅ Yes |
| **Changing a field number** | ❌ No |
| **Changing a field type** | ❌ No |
| **Changing a `repeated` field to non-repeated** | ❌ No |
| **Changing `oneof` fields** | ❌ No |

---

# Why Does a gRPC Client Have a Stub but Not a Server?
### **Why Does a gRPC Client Have a Stub but Not a Server?**
In gRPC, the **client** has a **stub**, but the **server does not** because of how **Remote Procedure Call (RPC) communication works**.

---

## **1️⃣ What is a Stub in gRPC?**
A **stub** is a **client-side proxy** that provides **remote access** to a gRPC service.  
- The **client calls methods on the stub**, which **internally handles**:
  - **Serializing** requests
  - **Sending them over the network**
  - **Receiving responses from the server**
  - **Deserializing responses**

### **Example: gRPC Stub in C++ (Client Side)**
```cpp
std::shared_ptr<grpc::Channel> channel = grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials());
std::unique_ptr<Greeter::Stub> stub = Greeter::NewStub(channel);
```
- `Greeter::NewStub(channel)` creates a **proxy** that allows the client to call **SayHello()** remotely.

---

## **2️⃣ Why Doesn't the Server Need a Stub?**
The **server directly implements the service** defined in the `.proto` file instead of calling it remotely.

- The **server receives requests directly**, so **it does not need a stub** to call itself.
- Instead, it **registers service implementations** and **waits for clients to call them**.

### **Example: gRPC Server Implementation (C++)**
```cpp
class GreeterServiceImpl final : public Greeter::Service {
public:
    Status SayHello(ServerContext* context, const HelloRequest* request, HelloResponse* reply) override {
        std::string message = "Hello, " + request->name();
        reply->set_message(message);
        return Status::OK;
    }
};

void RunServer() {
    GreeterServiceImpl service;
    ServerBuilder builder;
    builder.AddListeningPort("0.0.0.0:50051", grpc::InsecureServerCredentials());
    builder.RegisterService(&service);
    std::unique_ptr<Server> server(builder.BuildAndStart());
    server->Wait();
}
```
💡 The **server does not need a stub** because it implements the gRPC service **directly**.

---

## **3️⃣ Key Differences: Stub vs. Service**
| Feature | gRPC **Client Stub** | gRPC **Server Implementation** |
|---------|----------------------|-------------------------------|
| **Definition** | A **proxy object** that lets clients call remote methods | The **actual service implementation** that handles requests |
| **Who Uses It?** | The **client** (to call RPCs) | The **server** (to handle RPCs) |
| **How It Works?** | Calls a remote function | Implements the function locally |
| **How Is It Created?** | `Greeter::NewStub(channel)` | `class GreeterServiceImpl : public Greeter::Service` |
| **Network Role** | **Sends requests** to the server | **Receives and processes requests** |

---

## **4️⃣ Why the Client Needs a Stub**
Since the **client and server are separate**, the **client must call the server over the network**.

- The **stub makes remote calls appear like local function calls**.
- It **hides networking complexity** (serialization, deserialization, error handling, retries).
- The client **does not implement the actual function**, it just **calls it via the stub**.

---

## **5️⃣ Visual Representation**
```plaintext
       CLIENT SIDE                             SERVER SIDE
+---------------------------------+      +----------------------------------+
|  Application Code               |      |  Application Code                |
|  - GreeterClient                |      |  - GreeterServiceImpl            |
|  - Calls SayHello() via Stub     |      |  - Implements SayHello()         |
+---------------------------------+      +----------------------------------+
          |  (Call SayHello)                        |  (Process request)
          v                                        ^
+---------------------------------+      +----------------------------------+
| gRPC Stub (Client Proxy)        |      | gRPC Server Handler             |
|  - Serializes Request           |      |  - Deserializes Request         |
|  - Sends Request via HTTP/2      | ----> |  - Calls actual function       |
|  - Waits for Response            | <---- |  - Serializes Response        |
|  - Deserializes Response         |      |  - Sends Response via HTTP/2   |
+---------------------------------+      +----------------------------------+
          |                                        ^
          v                                        |
+---------------------------------+      +----------------------------------+
| gRPC Runtime & Network          |      | gRPC Runtime & Network           |
|  - Handles Connection           | ----> |  - Handles Connection           |
|  - Manages HTTP/2 Streams       | <---- |  - Manages HTTP/2 Streams      |
|  - Implements Load Balancing    |      |  - Implements Load Balancing    |
+---------------------------------+      +----------------------------------+

```
💡 The **stub** is **only needed on the client-side** because **the server directly implements the service**.

---

## **6️⃣ Summary**
| Feature | gRPC Client | gRPC Server |
|---------|------------|-------------|
| **Has a Stub?** | ✅ Yes (uses `Greeter::Stub`) | ❌ No (directly implements service) |
| **Calls Remote Methods?** | ✅ Yes (via stub) | ❌ No (just implements them) |
| **Needs Networking Logic?** | ✅ Yes (sends requests) | ✅ Yes (receives requests) |
| **Implements the RPCs?** | ❌ No (only calls them) | ✅ Yes (handles them) |

✅ **Clients use a stub to call remote services**.  
✅ **Servers directly implement the service without a stub**.  
✅ **Stubs allow remote calls to feel like local function calls**.  

# gRPC C++ Dependencies and Their Roles
### **gRPC C++ Dependencies and Their Roles**
When building gRPC for C++, the following dependencies are specified in your CMake command:

```sh
-DgRPC_ABSL_PROVIDER=package     \
-DgRPC_CARES_PROVIDER=package    \
-DgRPC_PROTOBUF_PROVIDER=package \
-DgRPC_RE2_PROVIDER=package      \
-DgRPC_SSL_PROVIDER=package      \
-DgRPC_ZLIB_PROVIDER=package
```

Each of these dependencies serves a **specific purpose** in the gRPC ecosystem. Below is an **introduction to each dependency and why gRPC needs it**.

---

## **1️⃣ ABSL (`abseil-cpp`) - Utility Library**
- **What it is:** A collection of C++ utilities developed by Google.
- **Why gRPC needs it:**  
  ✅ Provides essential C++ utilities for **strings, time handling, memory management, logging, and threading**.  
  ✅ Used instead of standard C++ utilities for **better performance and portability**.  

**Example Use in gRPC:**  
- `absl::Time` and `absl::Duration` for **efficient time handling**.
- `absl::Mutex` for **thread synchronization**.

🔹 **Without ABSL:** gRPC would need to use standard C++ utilities, which might not be as optimized.

---

## **2️⃣ c-ares (`c-ares`) - Asynchronous DNS Resolver**
- **What it is:** A C library for **asynchronous DNS resolution**.
- **Why gRPC needs it:**  
  ✅ Allows gRPC to **resolve domain names** (e.g., `grpc.example.com → IP address`) **without blocking the thread**.  
  ✅ Ensures **non-blocking, scalable** name resolution, which is essential for microservices.

**Example Use in gRPC:**  
- When a client calls `grpc::CreateChannel("example.com:50051", creds)`, gRPC uses **c-ares** to resolve `"example.com"` to an IP address **without blocking execution**.

🔹 **Without c-ares:** gRPC would need to rely on **blocking** system DNS lookups, which would slow down performance.

---

## **3️⃣ PROTOBUF (`protobuf`) - Serialization Library**
- **What it is:** Protocol Buffers (Protobuf) is a **serialization format** used by gRPC.
- **Why gRPC needs it:**  
  ✅ **Encodes and decodes structured data** in an **efficient, compact, binary format**.  
  ✅ **Defines messages** in `.proto` files and generates C++ classes for structured data exchange.

**Example Use in gRPC:**  
- Defines **RPC request & response messages**.
- Serializes them before **sending over the network** and deserializes them upon **receiving**.

**Example:**
```proto
message HelloRequest {
  string name = 1;
}
```
🔹 **Without Protobuf:** gRPC wouldn’t have a standard way to **define and serialize messages**.

---

## **4️⃣ RE2 (`re2`) - Regular Expression Library**
- **What it is:** A **fast, safe regular expression library** from Google.
- **Why gRPC needs it:**  
  ✅ Used internally for **parsing and handling text patterns efficiently**.  
  ✅ Unlike traditional regex engines, **RE2 does not use recursion**, making it safer and preventing stack overflows.

**Example Use in gRPC:**  
- Parsing **HTTP headers** and **URIs** in gRPC-Web.
- Matching **service names** and **metadata fields**.

🔹 **Without RE2:** gRPC would need to rely on **other regex libraries**, which may not be as optimized or safe.

---

## **5️⃣ SSL (`OpenSSL` or `BoringSSL`) - Security & Encryption**
- **What it is:** A cryptographic library for **TLS/SSL encryption**.
- **Why gRPC needs it:**  
  ✅ Enables **secure** gRPC connections (TLS over HTTP/2).  
  ✅ Provides **authentication, encryption, and integrity** for data exchange.

**Example Use in gRPC:**  
- Secure client-server communication:
```cpp
auto creds = grpc::SslCredentials(grpc::SslCredentialsOptions());
auto channel = grpc::CreateChannel("example.com:50051", creds);
```
🔹 **Without SSL:** gRPC would only support **plaintext (insecure) communication**, exposing data to interception.

---

## **6️⃣ ZLIB (`zlib`) - Compression Library**
- **What it is:** A widely-used **compression** library.
- **Why gRPC needs it:**  
  ✅ Reduces message **size**, improving **network efficiency**.  
  ✅ Helps in **compressing large RPC responses**.

**Example Use in gRPC:**  
- Compressing gRPC responses using **gzip**.
```cpp
grpc::ChannelArguments args;
args.SetCompressionAlgorithm(GRPC_COMPRESS_GZIP);
```
🔹 **Without Zlib:** gRPC would always send **uncompressed data**, leading to **higher bandwidth usage**.

---

## **🔍 Summary of Each Dependency and Its Role in gRPC**
| **Dependency** | **Purpose** | **Why gRPC Needs It?** |
|--------------|------------|------------------------|
| **ABSL (`abseil-cpp`)** | Utility library | Provides **memory management, time handling, and threading** tools. |
| **c-ares (`c-ares`)** | Asynchronous DNS | Enables **non-blocking** domain name resolution. |
| **PROTOBUF (`protobuf`)** | Serialization | **Encodes and decodes** structured gRPC messages. |
| **RE2 (`re2`)** | Regex Library | Efficient and **safe pattern matching**. |
| **SSL (`OpenSSL` or `BoringSSL`)** | Security | Enables **TLS encryption** for secure gRPC communication. |
| **ZLIB (`zlib`)** | Compression | Supports **message compression**, reducing network overhead. |

---

## **📌 Conclusion**
✅ **All these dependencies are required** to make gRPC **fast, secure, and scalable**.  
✅ **Each dependency serves a specific function**, such as **message serialization, networking, and security**.  
✅ **Removing any of these would break critical features** like **DNS resolution, encryption, or compression**.

