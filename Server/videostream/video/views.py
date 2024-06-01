# video/views.py

from django.shortcuts import render
from django.http import JsonResponse
from django.views.decorators.csrf import csrf_exempt
import os
import movenet
import uuid
from threading import Semaphore

# Your existing index view
def index(request):
    return render(request, 'index.html')
sem = Semaphore()

# Add the process_frame view below your existing views
@csrf_exempt
def process_frame(request):
    print("FILES:", request.FILES)  # Debugging to check the incoming files
    print("POST:", request.POST)  # Debugging to check the incoming POST data
    if request.method == 'POST':
        frame = request.FILES.get('frame')
        if not frame:
            return JsonResponse({'error': 'No frame provided'}, status=400)
        
        # Save the frame temporarily
        frame_path = f'temp_frame_{uuid.uuid4()}.jpg'
        with open(frame_path, 'wb+') as destination:
            for chunk in frame.chunks():
                destination.write(chunk)
        
        keypoints = None
        # Use the semaphore to protect the call to process_movenet_single_frame
        with sem:
            # This block is now protected by the semaphore and will only be executed
            # by one thread at a time within the same process
            keypoints = movenet.process_movenet_single_frame(frame_path)
    
        # Cleanup: remove the temporary file after processing
        os.remove(frame_path)
        
        # Return the keypoints as JSON
        print("keypoints: ")
        print(keypoints)
        return JsonResponse({'keypoints': keypoints})
    else:
        return JsonResponse({'error': 'Invalid request'}, status=400)

