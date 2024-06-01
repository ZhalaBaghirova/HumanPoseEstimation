# video/urls.py

from django.urls import path
from .views import index
from video.views import process_frame
urlpatterns = [
    path('', index, name='index'),
    path('process_frame/', process_frame, name='process_frame'),

]
