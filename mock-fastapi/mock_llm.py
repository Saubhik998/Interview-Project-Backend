# mock_llm.py
from fastapi import FastAPI, Request
from pydantic import BaseModel
from typing import Any, Dict

app = FastAPI()

class ChatRequest(BaseModel):
    model: str
    messages: list
    # … you can expand based on what your .NET client sends

class Choice(BaseModel):
    message: Dict[str, Any]
    finish_reason: str = "stop"
    index: int = 0

class ChatResponse(BaseModel):
    id: str = "mocked-id"
    object: str = "chat.completion"
    created: int = 0
    model: str
    usage: Dict[str, int] = {"prompt_tokens": 0, "completion_tokens": 0, "total_tokens": 0}
    choices: list[Choice]

@app.post("/v1/chat/completions", response_model=ChatResponse)
async def mock_chat(req: ChatRequest):
    # Always reply with the same fixed text:
    reply = {"content": "This is a mocked LLM response."}
    return ChatResponse(
        model=req.model,
        choices=[Choice(message=reply)]
    )
