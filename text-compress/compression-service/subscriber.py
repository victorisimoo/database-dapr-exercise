def handle_compression_result(body, headers):
    compressed_text = body.get('data', {}).get('compressed_text')
    print(f"Compressed text: {compressed_text}")

subscriptions = [
    {
        'pubsubname': 'pubsub',
        'topic': 'compression-result',
        'route': '/compress/result'
    }
]

def start_subscriber():
    from dapr.ext.grpc import App
    app = App()
    app.add_binding('compression-result', handle_compression_result)
    app.run(5000)

if __name__ == '__main__':
    start_subscriber()