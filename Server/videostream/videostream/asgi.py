"""
ASGI config for videostream project.

It exposes the ASGI callable as a module-level variable named ``application``.

For more information on this file, see
https://docs.djangoproject.com/en/4.2/howto/deployment/asgi/
"""

# videostream/asgi.py

import os
from django.core.asgi import get_asgi_application
from channels.routing import ProtocolTypeRouter, URLRouter
from django.urls import path
from video.consumers import VideoConsumer

os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'videostream.settings')

application = ProtocolTypeRouter({
    'http': get_asgi_application(),
    'websocket': URLRouter([
        path('ws/video/', VideoConsumer.as_asgi()),
    ]),
})
