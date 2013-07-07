#Motion Trackeeer [HACK!]

For every frame, fire event | handle event to:

+ Clone frame
+ Write clone to raw memstream
+ Generate bitmap from raw memstream + .png
+ Run bitmap through 
++ Custom Filters
++ Blob Analysis [Aforge.net]
+ Generate vectors from blob list
+ Merge blob vectors overlay into bitmap [load into canvas, draw, dump to mem]
+ Write merged bitmap into raw memstream
 
+ Generate bitmapImage from raw memstream
+ Push bitmapImage to main viewer

__tl;dr- Frame in | filter | blob | merge | Frame out__

NOTE!
Must manually explicitly set seek position to start of memstream before reading!


Kfouwels
