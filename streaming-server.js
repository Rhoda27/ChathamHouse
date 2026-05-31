const NodeMediaServer = require('node-media-server');

const config = {
  rtmp: {
    port: 1935,
    chunk_size: 60000,
    gop_cache: true,
    ping: 30,
    ping_timeout: 60
  },
  http: {
    port: 8000,
    allow_origin: '*',
    mediaroot: './media'
  },
  trans: {
    ffmpeg: 'ffmpeg',
    tasks: [
      {
        app: 'live',
        hls: true,
        hlsFlags: '[hls_time=2:hls_list_size=3:hls_flags=delete_segments]',
        dash: true,
        dashFlags: '[frag_duration=3:window_size=3:remove_at_exit=1]'
      }
    ]
  }
};

var nms = new NodeMediaServer(config);
nms.run();

console.log('========================================');
console.log('Streaming server started successfully!');
console.log('========================================');
console.log('RTMP Server: rtmp://localhost:1935/live');
console.log('HLS Server: http://localhost:8000/live/[stream_key].m3u8');
console.log('========================================');
console.log('Waiting for streamers to connect...');
console.log('Press Ctrl+C to stop the server');
console.log('========================================');
