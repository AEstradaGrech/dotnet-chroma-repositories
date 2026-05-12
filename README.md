# 🚀 dotnet-chroma-repositories

[![NuGet Version](https://img.shields.io/nuget/v/Estrada.ChromaDB.Repositories)](https://www.nuget.org/packages/Estrada.ChromaDB.Repositories)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0%2B-purple.svg)](https://dotnet.microsoft.com/)

> Lightweight ChromaDB repository SDK for .NET that wraps around the Semantic Kernel Microsoft.SemanticKernel.Connectors.Chroma (for now).

This is the repository for the **Estrada.ChromaDB.Repositories** NuGet package. 🎉

## 📋 Table of Contents

- [What's in the Box](#whats-in-the-box)
- [⚠️ Important Notes](#️-important-notes)
- [🔍 How Does It Work](#-how-does-it-work)
- [📚 Class Details](#-class-details)
- [🛠️ How to Use](#️-how-to-use)
- [📖 Implementation Sample](#-implementation-sample)

## 📦 What's in the Box

- 🏗️ A lightweight framework that splits the Chroma data model into **COLLECTIONS** and **CHUNKS**, enabling more C#-friendly interactions with Chroma data.
- 📚 A repository template of `<TCollection, TChunk>` for seamless access to Chroma and easy manipulation of these structures.
- 🔧 A default implementation sample, ready to register, inject, and use.
- ✨ Additional features missing from the Microsoft.SemanticKernel.Connectors.Chroma package:
  - 📄 Document property binding
  - 🔍 Metadata filtering
  - 📄 Pagination options

## ⚠️ Important Notes

> **Prerequisites:**
> - **Package Version:** `Microsoft.SemanticKernel.Connectors.Chroma --version 1.74.0-alpha`
> - **Chroma Image:** Use exactly `chromadb/chroma:0.4.24`
> - **Docker Command:** `docker run --name local-chroma-container -v ./chroma-data:/data -p XXXX:8000 chromadb/chroma:0.4.24`

📝 **Notes:**
- Future versions plan to remove the Semantic Kernel dependency, but it's required for now.
- Use a Docker volume to persist Chroma data. Create a local folder as a 'bridge' to the container and run the command from there.

## 🔍 How Does It Work

The SDK utilizes these core classes to handle ChromaDB data:

- **COLLECTION**: A grouping structure for chunks and summary/common data.
- **CHUNK**: A unit of data associated with a collection. May include an embedding or be text-only.
- **METADATA**: Maps Chroma metadata to a typed class, defining the shape of a chunk or collection.
- **REPOSITORY**: The Chroma connector, handling access and defining types for `TCol` and `TChunk`.

### 🏗️ Structures Hierarchy

- Repository<`TCol`, `TChunk`> of:
    - Collections<`TCol`> with:
        - 1 x CollectionMetadata<`TCol`>
        - N x Chunks<`TChunk`> with:
            - 1 x ChunkMetadata<`TChunk`> 


## 📚 Class Details

| Class | Description |
|-------|-------------|
| **COLLECTIONS** 🗂️ | Data structure holding summary data common to all collection chunks.<br>- Always the first chunk (ID = 0).<br>- No text embedding, only a description.<br>- Stores: Model + Dimensions, Total Chunks, Type & Chunk Type. |
| **CHUNKS** 📄 | Pieces of data to query, with desired shape/metadata.<br>- ID assigned on creation.<br>- May or may not have embeddings.<br>- Includes DefaultMetadata class for typed access. |
| **METADATA** 🏷️ | Maps metadata to C# classes, defining chunk/collection shapes.<br>- Used in constructors for deserialization.<br>- Ensures correct key mapping via `AddMetadata`. |
| **REPOSITORIES** 🔌 | Connects to Chroma, reshapes data per types.<br>- Isolates client access.<br>- Includes extensions for filtering, document binding, pagination. |

> **Note:** Collections and Chunks have distinct Metadata classes. Extend `CollectionMetadata` & `ChunkMetadata` accordingly.

## 🛠️ How to Use

Let's create a repository for embedding multiple files in the same collection. 📁

### Step 0: Setup
Use the `AddChromaClient` and optionally `AddChromaConfiguration` startup extensions to register the client and inject `ChromaSettings` from `appsettings.json`. Also, register your repo.

### Steps:
1. **Create Collection Class** 📂<br>Create a class inheriting from `ChromaCollection` (e.g., `ChromaFilesCollection`).
2. **Create Collection Metadata** 🏷️<br>Inherit from `ChromaCollectionMetadata` (e.g., `FilesCollectionMetadata`).
3. **Create Chunk Class** 📄<br>Inherit from `ChromaChunk` (e.g., `ChromaFileChunk`).
4. **Create Chunk Metadata** 🏷️<br>Inherit from `ChromaMetadata` (e.g., `FileChunkMetadata`).
5. **Create Repository** 🔌<br>Inherit from `ChromaRepository<TCol, TChunk>` (e.g., `ChromaFilesRepository<ChromaFilesCollection, ChromaFileChunk>`).<br>*Recommended:* Create an interface extending `IChromaRepository<TCol, TChunk>`.
6. **Create Enum** 🔢<br>Define an enum for TYPE and CHUNK_TYPE (start from 2 to avoid reserved values 0 and 1).

   ```csharp
   enum EChunkType {
       FILE = 2,
       CUSTOMER_REVIEWS = 3,
       // ...
   }
   ```

   - Collections get TYPE = 'COLLECTION' and CHUNK_TYPE from your enum.
   - Chunks get TYPE = your enum value for filtering.

   *Tip:* Use `DefaultChunk(string collectionName)` for new chunks with predefined settings.

## 📖 Implementation Sample

This repository originates the package and serves as a sample implementation: [dotnet-llamasharp](https://github.com/AEstradaGrech/dotnet-llamasharp)

It demonstrates handling Files, Chats, and System messages for a RAG application using the package's Default Implementation for generic file inspection.

---

Made with ❤️ for the .NET community. Contributions welcome! 🌟
