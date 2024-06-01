import tensorflow as tf
import numpy as np
import cv2

# Assuming the MoveNet model is loaded globally
interpreter = tf.lite.Interpreter(model_path='lite-model_movenet_singlepose_lightning_3.tflite')
interpreter.allocate_tensors()

def process_movenet_single_frame(image_path):
    # Load the image
    frame = cv2.imread(image_path)
    
    # Preprocess the image
    img = tf.image.resize_with_pad(np.expand_dims(frame, axis=0), 192, 192)
    input_image = tf.cast(img, dtype=tf.float32)
    # Make predictions
    input_details = interpreter.get_input_details()
    output_details = interpreter.get_output_details()
    interpreter.set_tensor(input_details[0]['index'], np.array(input_image))
    interpreter.invoke()
    keypoints_with_scores = interpreter.get_tensor(output_details[0]['index'])
    
    # Normally, here you'd draw keypoints and connections directly on the frame
    # But for a Django response, you might want to return the raw data instead
    return keypoints_with_scores.tolist()
