from fastapi import FastAPI
from pydantic import BaseModel
from fastapi.responses import JSONResponse
import uvicorn

app = FastAPI()

class GeminiRequest(BaseModel):
    prompt: str

@app.post("/gemini/ask")
async def mock_gemini_response(req: GeminiRequest):
    return JSONResponse(content={
        "response": [
            {"text": "Mock question 1"},
            {"text": "Mock question 2"},
            {"text": "Mock question 3"}
        ]
    })

if __name__ == "__main__":
    uvicorn.run(app, host="localhost", port=8000)
