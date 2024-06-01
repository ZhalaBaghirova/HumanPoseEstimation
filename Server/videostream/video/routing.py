# video/routing.py

from channels.routing import ProtocolTypeRouter, URLRouter
from django.urls import path
from .consumers import VideoConsumer

application = ProtocolTypeRouter({
    'websocket': URLRouter([
        path('ws/video/', VideoConsumer.as_asgi()),
    ]),
})
