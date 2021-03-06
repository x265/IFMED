# IFME Distribution
## What is this?
This command-line tool allow you encode video over network (via pipe).

Most encoder's support `stdin` and `stdout`, most popular tool is FFmpeg

## How to use?
### System Requirement
Ensure your computer meets the following specification:

1. .NET Framework 4.0 (Windows) or Mono Runtime (Linux)
2. Windows Vista and above or Ubuntu 14.04 and above
3. Intel Core i5 (1st Gen) or AMD AthlonII X2
4. 1GB RAM for handling RAW frame

### Example
```
Internet Friendly Media Encoder Distribution - by Anime4000 (x265.github.io)
Version: 0.1.5579.20146 compiled on 11/Apr/2015 (AMD64/Windows RELEASE)

Lets you encode video over network, splits frames for each server
and equally distributes jobs to available workers in parallel.

Usage: ifmed [--help] [-s/-c] [-h ip] [-p port] [< "nope.avi"]
       ifmed [--help] [-s/-c] [-h ip] [-p port] [| ffmpeg.exe]

Options:
   --help   Display help
   -s       Run as server
   -c       Run as client
   -h       IP Address (Default: 127.0.0.1/All interface)
   -p       TCP Port (Range: 1024 - 65535. Default: 4000)
   -        stdin/stdout

Example:
   Client mode, distribute stream
   ifmed -c -h 192.168.1.2 - < "nope.avi"
   ifmed -c -h 192.168.1.2 -p 4001 - < "nope.avi"
   avs2pipe video "nope.avs" | ifmed -c -h 192.168.1.2 -

   Server mode, encode stream
   ifmed -s - | ffmpeg -i - nope.mp4
   ifmed -s -h 192.168.1.2 - | ffmpeg -i - nope.mp4
   ifmed -s -h 192.168.1.2 -p 4001 - | ffmpeg -i - nope.mp4

   Server mode, receive stream with specific time and duration
   ifmed -s - | ffmpeg -i - -ss 120 -t 200 nope.mp4

Note:
1. Server needs to be run prior to client.
2. Pipe over WAN is not recommended unless
   you have speeds exceeding 10mbps with VPN (encryption).
3. Sending RAW stream over network is not recommended unless you have 10Gbps.
```

### Detailed Explanation
#### Server mode
Server mode is when a machine only does encoding. Before starting a client, ensure the server is running and awaiting connection.

FFmpeg:
```
user@host~# ifmed -s - | ffmpeg -i - nope.mp4
```

FFmpeg (listen specific interface):
```
user@host~# ifmed -s -h 192.168.1.3 - | ffmpeg -i - nope.mp4
```

FFmpeg (listen specific interface and port):
```
user@host~# ifmed -s -h 192.168.1.3 -p 4001 - | ffmpeg -i - nope.mp4
```

Or you can do more pipe! FFmpeg to x265!:
```
user@host~# ifmed -s - | ffmpeg -i - -pix_fmt yuv420p -f yuv4mpegpipe - 2> nul | x265 -p ultrafast --crf 25 -t psnr -o nope.hevc --y4m -
```

or encode specific start time and duration:
```
user@host~# ifmed -s - | ffmpeg -i - -pix_fmt yuv420p -f yuv4mpegpipe -ss 120 -t 240 - 2> /dev/nul | x265 -p ultrafast --crf 25 -t psnr -o nope.hevc --y4m -
```

#### Client mode
Client mode is the host that sending video data to encoding server. first you need a server running, see below.

To send a video (localhost default):
```
user@host~# ifmed -c - < "nope.avi"
```

Send to server:
```
user@host~# ifmed -c -h 192.168.1.3 - < "nope.avi"
```

With specific port:
```
user@host~# ifmed -c -h 192.168.1.3 -p 4001 - < "nope.avi"
```

Or send RAW (AviSynth) not recommended:
```
user@host~# avs2pipemod video "nope.avs" | ifmed -c -h 192.168.1.3 -p 4001 -
```

#### FAQ
Notice: `2> /dev/nul` (linux) or `2> nul` (windows) not to display text.

Notice: `-ss` is FFmpeg command for Start encode at specific second.

Notice: `-t` is FFmpeg command for Total time encode, require `-ss`.

## License
All under GNU GPL v2.

Commercial use is not allowed for the moment.

遥声
