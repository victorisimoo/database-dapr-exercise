from flask import Flask, request
import requests
import json

app = Flask(__name__)

@app.route('/compress', methods=['POST'])
def compress_text():
    text = request.json.get('text')
    headers = {'Content-Type': 'application/json'}
    payload = json.dumps({
        'data': {
            'text': text
        }
    })
    requests.post('http://localhost:3500/v1.0/publish/pubsub/text-compression', headers=headers, data=payload)
    return {'result': 'Text sent for compression'}

if __name__ == '__main__':
    app.run(port=3000)