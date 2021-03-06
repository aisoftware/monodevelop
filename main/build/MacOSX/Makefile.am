MAC_APP_DIR=MonoDevelop.app
APP_NAME=MonoDevelop
APP_DISPLAY_NAME=MonoDevelop
MACOS=$(MAC_APP_DIR)/Contents/MacOS
LIB_DIR=$(MAC_APP_DIR)/Contents/Resources/lib
MAC_APP_LIB_DIR=$(LIB_DIR)/monodevelop
ADDINS=$(MAC_APP_LIB_DIR)/AddIns
EXTRAS=$(top_srcdir)/../extras
ARTIFACTS=$(top_srcdir)/../../artifacts
MD_CONFIGURE=$(top_srcdir)/../scripts/configure.sh
EXTERNAL=../../external
MONOSTUB_STATIC_LINK=$(EXTERNAL)/Xamarin.Mac.registrar.full.a

if !RELEASE_BUILDS
HYBRID_SUSPEND_ABORT=-DHYBRID_SUSPEND_ABORT
endif

#SDK_PATH=$(shell xcrun --sdk macosx10.8 --show-sdk-path)

PACKAGE_UPDATE_ID=$(shell $(MD_CONFIGURE) get-releaseid)
BUNDLE_VERSION=$(shell $(MD_CONFIGURE) get-version)

CLEANFILES = render.exe monostub monostub-test
#DISTCLEANFILES =
EXTRA_DIST = dmg-bg.png DS_Store Info.plist.in make-dmg-bundle.sh render.cs

MONOSTUB_EXTRA_SOURCEFILES = monostub-utils.h
export MACOSX_DEPLOYMENT_TARGET=10.13

# With the hardened runtime, we need to specify the location of all libraries
# that we dlopen
MONOSTUB_RPATH=-Wl,-rpath,/Library/Frameworks/Mono.framework/Libraries/ \
-Wl,-rpath,@executable_path/../Resources/lib/monodevelop/bin/ \
-Wl,-rpath,@executable_path/../Resources/lib/ \
-Wl,-rpath,@executable_path/../Resources/lib/monodevelop/AddIns/DisplayBindings/TextEditor.Cocoa/ \
-Wl,-rpath,@executable_path/../Resources/lib/monodevelop/AddIns/VersionControl/ \
-Wl,-rpath,@executable_path/../Resources/lib/monodevelop/AddIns/VersionControl/lib/osx/ \
-Wl,-rpath,/Library/Developer/CommandLineTools/usr/lib/ \
-Wl,-rpath,/Applications/Xcode.app/Contents/Developer/ \
-Wl,-rpath,/usr/lib/ \
-Wl,-rpath,/usr/local/lib/

all: monostub monostub-nogui monostub-test debugcrash

render.exe: render.cs
	mcs -r:System.Drawing render.cs

dmg: render.exe app
	./make-dmg-bundle.sh

if !RELEASE_BUILDS
debugcrash: libdebugcrash.dylib
	cp $^ ../bin
else
debugcrash:
endif

if !RELEASE_BUILDS
libdebugcrash.dylib: debugcrash.c
	clang -g -shared -fpic -o $@ $^
endif

monostub.o: monostub.mm $(MONOSTUB_EXTRA_SOURCEFILES)
	g++ -g $(HYBRID_SUSPEND_ABORT) $(ENABLE_DEBUG_CRASHER) -c -Wall -m64 -o $@ monostub.mm

monostub-nogui.o: monostub.mm $(MONOSTUB_EXTRA_SOURCEFILES)
	g++ -g $(HYBRID_SUSPEND_ABORT) -DNOGUI -c -Wall -m64 -o $@ monostub.mm

monostub: monostub.o $(MONOSTUB_STATIC_LINK)
	clang++ -g -Wall -m64 -o $@ -Wl,-all_load $^ -framework AppKit -framework Quartz -undefined dynamic_lookup $(MONOSTUB_RPATH)
	mkdir -p ../bin
	cp $@ ../bin/MonoDevelop

monostub-nogui: monostub-nogui.o $(MONOSTUB_STATIC_LINK)
	clang++ -g -Wall -m64 -o $@ -Wl,-all_load $^ -framework AppKit -framework Quartz -undefined dynamic_lookup $(MONOSTUB_RPATH)
	mkdir -p ../bin
	cp $@ ../bin/mdtool

monostub-test: monostub-test.m $(MONOSTUB_EXTRA_SOURCEFILES)
	gcc -g -Wall -m64 -o $@ monostub-test.m -framework AppKit
	./monostub-test

clean-local:
	rm -rf MonoDevelop.app
	rm -f MonoDevelop*.dmg
	rm -f *.o

app: monostub monostub-test
	@echo ""
	@echo "Creating directories in app bundle"
	@echo ""
	mkdir -p $(MAC_APP_DIR)/Contents/{MacOS,Resources}
	mkdir -p $(MAC_APP_DIR)/Contents/MacOS/share
	mkdir -p $(MAC_APP_LIB_DIR)

#	@echo ""
#	@echo "Copying install prefix into app bundle"
#	@echo ""
#	@if [[ "$(prefix)" == /Library* ]] || [[ "$(prefix)" == /usr* ]]; then \
#		echo "WARNING: cannot merge system prefix $(prefix)"; \
#	else \
#		echo "cp -pR $(prefix)/* $(MAC_APP_DIR)/Contents/MacOS/"; \
#		cp -pR $(prefix)/* $(MAC_APP_DIR)/Contents/MacOS/; \
#	fi

	@echo ""
	@echo "Copying MonoDevelop into app bundle"
	@echo ""
	cp -pR ../AddIns $(MAC_APP_LIB_DIR)
	cp -pR ../data $(MAC_APP_LIB_DIR)
	cp -pR ../bin $(MAC_APP_LIB_DIR)
	cp -pR ../locale $(MAC_APP_DIR)/Contents/MacOS/share
	mkdir -p $(MAC_APP_DIR)/Contents/MacOS/share/monodevelop
	cp ../../COPYING $(MAC_APP_DIR)/Contents/MacOS/share/monodevelop/COPYING.LGPL2.1

	sed -e "s/@BUNDLE_VERSION@/$(BUNDLE_VERSION)/" -e "s/@APP_NAME@/$(APP_NAME)/" -e "s/@APP_DISPLAY_NAME@/$(APP_DISPLAY_NAME)/" -e "s|@RELEASE_ID@|$(PACKAGE_UPDATE_ID)|" Info.plist.in > $(MAC_APP_DIR)/Contents/Info.plist
	cp Entitlements.plist $(MAC_APP_DIR)/Contents/Entitlements.plist
	cp ../../theme-icons/Mac/*.icns $(MAC_APP_DIR)/Contents/Resources/

# Native launch scripts
	@echo ""
	@echo "Copying launch scripts into app bundle"
	@echo ""
	cp monostub $(MACOS)/MonoDevelop
	cp monostub $(MACOS)/mdtool

# Remove addins that don't work on OSX
	@echo ""
	@echo "Removing unsupported addins"
	@echo ""

	rm -rf  $(ADDINS)/GnomePlatform* \
	  $(ADDINS)/WindowsPlatform*

# remove stuff we don't need
	@echo ""
	@echo "Removing unneeded files"
	@echo ""

	rm -rf $(MAC_APP_LIB_DIR)/bin/UnitTests.dll* \
	  $(MAC_APP_LIB_DIR)/bin/TestResult.xml \
	  $(MAC_APP_LIB_DIR)/bin/addin-db* \
	  $(MAC_APP_LIB_DIR)/bin/local \
	  $(MAC_APP_LIB_DIR)/bin/repository-cache* \
	  $(MAC_APP_LIB_DIR)/bin/addins-setup.config \
	  $(MAC_APP_LIB_DIR)/AddIns/MonoDevelop.TextTemplating/Mono.TextTemplating.Tests*

# add the binary artifacts if they exist
	@echo ""
	@echo "Copying binary artifacts into app bundle"
	@echo ""

	mkdir -p $(LIB_DIR)/pkgconfig
	mkdir -p $(MACOS)/MonoDoc.app/
	test ! -d $(ARTIFACTS)/MonoDoc.app ||  cp -pR $(ARTIFACTS)/MonoDoc.app/* $(MACOS)/MonoDoc.app/

	cp -r MDMonitor.app $(MACOS)

# update revision in updateinfo
	@echo ""
	@echo "Updating build information"
	@echo ""

	$(MD_CONFIGURE) gen-buildinfo "$(MAC_APP_LIB_DIR)/bin"

# touch the directory so that update dependencies work
	touch MonoDevelop.app

	@echo ""
	@cat "$(MAC_APP_LIB_DIR)/bin/buildinfo"
	@if test -f missing-stuff; then echo ""; echo "Missing stuff:"; cat missing-stuff; fi
	@echo ""
	@rm -f missing-stuff

.PHONY: app dmg debugcrash
