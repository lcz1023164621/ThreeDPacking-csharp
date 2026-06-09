

GXX = g++

#	select Debug or Release 
OPTI = -o3 -s
#OPTI = -o0 -g
###########################

TARGET_NAME = GrabImage

PLATFORM = linux64

INCLUDE_PATH = -I/usr/include \
-I../../../../Include


SRC_PATH = ./$(TARGET_NAME).cpp


DLIB_PATH = ../../../../Libs/$(PLATFORM)


DLIB = -lMvCodeReaderCtrl \
-ldl \
-lpthread \
-lm \
-lstdc++


OUT_PATH = ./


#	Configuration Options
CFLAGS = -Wall -std=c++11 -Wno-unused-function -Wno-unused-variable $(OPTI) $(INCLUDE_PATH)
LDFLAGS = -Wl,-rpath=$(DLIB_PATH) -Wl,-Bsymbolic



TARGET_PATH = $(addprefix $(OUT_PATH), $(TARGET_NAME))


$(TARGET_PATH) : $(SRC_PATH)
	$(GXX) $(CFLAGS) $(LDFLAGS) -o $@ $(SRC_PATH) $ -L$(DLIB_PATH) $(DLIB)
	
	
.PHONY:clean
clean:
	rm -rf $(TARGET_PATH)

