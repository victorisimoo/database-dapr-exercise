import lzstring
from flask import Flask, request
import json
import requests

app = Flask(__name__)
compressor = lzstring.LZString()

@app.route('/compress', methods=['POST'])
def compress_text():
    text = request.json.get('text')
    compressed_text = compressor.compress(text)

    headers = {'Content-Type': 'application/json'}
    payload = json.dumps({
        'data': {
            'compressed_text': compressed_text
        }
    })
    requests.post('http://localhost:3500/v1.0/publish/pubsub/compression-result', headers=headers, data=payload)
    return {'result': 'Text compressed'}

def main():
    app.run(port=3001)

if __name__ == '__main__':
    main()