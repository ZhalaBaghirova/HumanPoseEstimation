# video/consumers.py

import json
from channels.generic.websocket import AsyncWebsocketConsumer

class VideoConsumer(AsyncWebsocketConsumer):
    async def connect(self):
        await self.accept()

    async def disconnect(self, close_code):
        pass

    async def receive(self, text_data):
        text_data_json = json.loads(text_data)

        command = text_data_json['command']

        if command == 'start':
            await self.send(text_data=json.dumps({'command': 'start'}))
        elif command == 'stop':
            await self.send(text_data=json.dumps({'command': 'stop'}))
