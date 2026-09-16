# RagCrmWorkspace
Document Intelligence CRM (RAG System)
This full-stack Retrieval-Augmented Generation (RAG) application allows users to securely upload business documents, embed them into a vector database, and extract insights using conversational AI.
Tech Stack & Architecture
⚬	Backend: .NET 8 Web API, C#, Entity Framework Core, and Microsoft Resilience (Polly).
⚬	Frontend: React, Vite, React Router, and Tailwind CSS.
⚬	Database: Dockerized PostgreSQL utilizing the pgvector extension for high-dimensional vector storage.
⚬	AI Integration: Google Gemini API (gemini-embedding-001 for vectorization, gemini-3.8-flash for text generation).
Core Features
⚬	Secure Access: JSON Web Token (JWT) session management and BCrypt password hashing.
⚬	Ingestion Pipeline: Automated document parsing, text chunking, and 768-dimensional vector generation.
⚬	Semantic Search: HNSW indexed vector similarity search using Cosine Distance mathematical calculations directly in the database.
⚬	Contextual Generation: Strict prompt engineering to prevent LLM hallucination, complete with source document citations.

