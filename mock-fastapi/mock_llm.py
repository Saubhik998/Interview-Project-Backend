# mock_llm.py

from fastapi import FastAPI, Request
from pydantic import BaseModel
from typing import Any, Dict, List

app = FastAPI()


class ChatRequest(BaseModel):
    """
    Request model for chat completions.
    
    Attributes:
        model (str): The name or identifier of the LLM model.
        messages (list): A list of message objects representing the chat context/conversation history.
    """
    model: str
    messages: list
    # Expand fields if your .NET client sends more keys.


class Choice(BaseModel):
    """
    Represents a single LLM completion choice.

    Attributes:
        message (dict): The message content returned by the LLM.
        finish_reason (str): The reason the LLM stopped generating (default: "stop").
        index (int): Index of this choice in the completions list.
    """
    message: Dict[str, Any]
    finish_reason: str = "stop"
    index: int = 0


class ChatResponse(BaseModel):
    """
    Response model for chat completions.

    Attributes:
        id (str): Unique identifier for the completion response (mocked).
        object (str): Object type (e.g., 'chat.completion').
        created (int): Timestamp (mocked as 0).
        model (str): Model name used for completion.
        usage (dict): Token usage statistics.
        choices (list[Choice]): List of completion choices.
    """
    id: str = "mocked-id"
    object: str = "chat.completion"
    created: int = 0
    model: str
    usage: Dict[str, int] = {"prompt_tokens": 0, "completion_tokens": 0, "total_tokens": 0}
    choices: List[Choice]


@app.post("/v1/chat/completions", response_model=ChatResponse)
async def mock_chat(req: ChatRequest):
    """
    Mock endpoint that simulates an LLM chat completion.

    Always responds with a static ('mocked') LLM output in the required OpenAI-style format.

    Args:
        req (ChatRequest): The incoming chat completion request payload.

    Returns:
        ChatResponse: The mocked response containing fixed text content.
    """
    # Always reply with the same fixed text:
    reply = {"content": "This is a mocked LLM response."}
    return ChatResponse(
        model=req.model,
        choices=[Choice(message=reply)]
    )
