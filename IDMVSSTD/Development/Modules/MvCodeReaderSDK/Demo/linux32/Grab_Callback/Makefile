PLATFORM = linux32
DLIB_PATH = ../../../../Libs/$(PLATFORM)

Demo:Grab_ImageCallBack.c
	g++  Grab_ImageCallBack.c  -o Grab_ImageCallBack -g  -Wall  -m32 -I../../../../Include  -L$(DLIB_PATH) -Wl,-Bdynamic -lMvCodeReaderCtrl  -lpthread -ldl -Wl,-rpath=$(DLIB_PATH)

clean:
	rm Grab_ImageCallBack -rf
