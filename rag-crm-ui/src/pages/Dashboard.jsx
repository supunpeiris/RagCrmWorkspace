import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

export default function Dashboard() {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);
  
  // State for Chat and Uploads
  const [prompt, setPrompt] = useState('');
  const [chatHistory, setChatHistory] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);

  // Helper to get the token
  const getToken = () => localStorage.getItem('token');

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigate('/login');
  };

  // 1. Handle Document Upload
  const handleFileUpload = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    setIsProcessing(true);
    const formData = new FormData();
    formData.append('file', file); // Must match the C# parameter name!

    try {
      const response = await fetch('http://localhost:5233/api/document/upload', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${getToken()}`
          // Note: Do NOT set Content-Type for FormData, the browser sets the boundary automatically
        },
        body: formData
      });

      if (response.ok) {
        const data = await response.json();
        setChatHistory(prev => [...prev, { 
          role: 'system', 
          content: `Success! ${file.name} chunked into ${data.totalChunks} pieces and embedded in PostgreSQL.` 
        }]);
      } else {
        throw new Error('Upload failed');
      }
    } catch (error) {
      console.error(error);
      setChatHistory(prev => [...prev, { role: 'system', content: '❌ Failed to upload document.' }]);
    } finally {
      setIsProcessing(false);
      // Reset file input so you can upload the same file again if needed
      event.target.value = null; 
    }
  };

  // 2. Handle RAG Search
  const handleSearch = async (e) => {
    e.preventDefault();
    if (!prompt.trim()) return;

    // Add user message to UI immediately
    const userMessage = prompt;
    setPrompt('');
    setChatHistory(prev => [...prev, { role: 'user', content: userMessage }]);
    setIsProcessing(true);

    try {
      const response = await fetch('http://localhost:5233/api/document/search', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${getToken()}`
        },
        body: JSON.stringify({ prompt: userMessage })
      });

      if (response.ok) {
        const data = await response.json();
        setChatHistory(prev => [...prev, { 
          role: 'ai', 
          content: data.answer,
          sources: data.sources
        }]);
      } else {
        throw new Error('Search failed');
      }
    } catch (error) {
      console.error(error);
      setChatHistory(prev => [...prev, { role: 'system', content: '❌ Search request failed.' }]);
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="flex h-screen bg-slate-50">
      
      {/* Sidebar Navigation */}
      <div className="w-64 bg-slate-900 text-white flex flex-col">
        <div className="p-4 text-2xl font-bold border-b border-slate-700">
          RAG CRM
        </div>
        <div className="flex-1 p-4">
          <nav className="space-y-2">
            <a href="#" className="block px-4 py-2 bg-blue-600 rounded">Intelligence Chat</a>
            <a href="#" className="block px-4 py-2 hover:bg-slate-800 rounded transition">Database</a>
          </nav>
        </div>
        <div className="p-4 border-t border-slate-700">
          <button 
            onClick={handleLogout} 
            className="w-full bg-slate-800 hover:bg-slate-700 text-left px-4 py-2 rounded transition"
          >
            Log Out
          </button>
        </div>
      </div>

      {/* Main Workspace */}
      <div className="flex-1 flex flex-col">
        
        {/* Top Header */}
        <header className="bg-white shadow-sm p-4 flex justify-between items-center">
          <h2 className="text-xl font-semibold text-slate-800">Document Intelligence</h2>
          
          {/* Hidden File Input & Upload Button */}
          <input 
            type="file" 
            ref={fileInputRef} 
            onChange={handleFileUpload} 
            className="hidden" 
            accept=".txt,.pdf,.csv"
          />
          <button 
            onClick={() => fileInputRef.current.click()}
            disabled={isProcessing}
            className="bg-green-600 hover:bg-green-700 disabled:bg-slate-400 text-white font-medium px-4 py-2 rounded shadow transition"
          >
            {isProcessing ? 'Processing...' : '+ Upload Document'}
          </button>
        </header>

        {/* Chat Interface */}
        <main className="flex-1 p-6 overflow-hidden flex flex-col">
          <div className="bg-white rounded-xl shadow-sm border border-slate-200 h-full flex flex-col overflow-hidden">
            
            {/* Chat History Area */}
            <div className="flex-1 p-6 overflow-y-auto space-y-4 bg-slate-50">
              {chatHistory.length === 0 ? (
                <p className="text-slate-500 text-center mt-10">
                  Upload a business document to start searching.
                </p>
              ) : (
                chatHistory.map((msg, index) => (
                  <div key={index} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                    <div className={`max-w-[80%] p-4 rounded-xl shadow-sm ${
                      msg.role === 'user' ? 'bg-blue-600 text-white' : 
                      msg.role === 'system' ? 'bg-slate-200 text-slate-700 text-sm font-mono' : 
                      'bg-white border border-slate-200 text-slate-800'
                    }`}>
                      <p className="whitespace-pre-wrap">{msg.content}</p>
                      
                      {/* Render Source Badges if AI provided them */}
                      {msg.sources && msg.sources.length > 0 && (
                        <div className="mt-3 pt-3 border-t border-slate-100 flex flex-wrap gap-2">
                          <span className="text-xs font-semibold text-slate-500">Sources:</span>
                          {msg.sources.map((src, i) => (
                            <span key={i} className="text-xs bg-slate-100 text-slate-600 px-2 py-1 rounded">
                              {src}
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                ))
              )}
            </div>
            
            {/* Chat Input Area */}
            <div className="p-4 border-t border-slate-200 bg-white">
              <form onSubmit={handleSearch} className="flex gap-2">
                <input 
                  type="text" 
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder="Ask about your documents..." 
                  disabled={isProcessing}
                  className="flex-1 border border-slate-300 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-slate-100" 
                />
                <button 
                  type="submit"
                  disabled={isProcessing || !prompt.trim()}
                  className="bg-blue-600 text-white font-medium px-6 py-2 rounded-lg hover:bg-blue-700 transition disabled:bg-slate-400"
                >
                  Search
                </button>
              </form>
            </div>

          </div>
        </main>
        
      </div>
    </div>
  );
}
