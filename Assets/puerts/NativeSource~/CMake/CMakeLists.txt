# Specify the minimum version for CMake
cmake_minimum_required(VERSION 3.2)

# Plugins's name
project(${PLUGIN_NAME})

set (CMAKE_CXX_STANDARD 14)

#Build type
set(CMAKE_BUILD_TYPE ${CMAKE_BUILD_TYPE})

#Plugin version
add_definitions(-DPLUGIN_VERSION="${PLUGIN_VERSION}")

#Plugin build number
add_definitions(-DPLUGIN_BUILD_NUMBER=${PLUGIN_BUILD_NUMBER})

#Custom definitions
if(DEFINED CUSTOM_DEFINES)
  set(CUSTOM_DEFINES ${CUSTOM_DEFINES})
  foreach(definition ${CUSTOM_DEFINES})
    add_definitions(-D${definition})
  endforeach()
endif()

file(STRINGS "../Source/quickjs/VERSION" QJS_VERSION_STR)

#Include Unity plugin API
if(DEFINED INCLUDE_PLUGIN_API)
  include_directories(${INCLUDE_PLUGIN_API})
endif()

set(ThirdParty ${CMAKE_SOURCE_DIR}/../third/ThirdParty)
set(WEBSOCKET_INC ${ThirdParty}/Include/websocketpp)
set(ASIO_INC ${ThirdParty}/Include/asio)

include_directories(
    ${CMAKE_SOURCE_DIR}/../Source
    ${CMAKE_SOURCE_DIR}/../Source/quickjs
    ${CMAKE_SOURCE_DIR}/../Source/include
    ${CMAKE_SOURCE_DIR}/../Source/Inc
    ${CMAKE_SOURCE_DIR}/../third/Source/JsEnv/Private
    ${CMAKE_SOURCE_DIR}/../third/Source/JsEnv/Private/V8InspectorImpl.h
    ${CMAKE_SOURCE_DIR}/../third/Source/JsEnv/Private/PromiseRejectCallback.hpp
    ${WEBSOCKET_INC}
    ${ASIO_INC}
    
)


# Find all .c, .cc and .cpp files in the source folder
file(GLOB_RECURSE SOURCE_FILES "${SOURCE_FOLDER}/*.c" "${SOURCE_FOLDER}/*.cc" "${SOURCE_FOLDER}/*.cpp" "${CMAKE_SOURCE_DIR}/../third/Source/JsEnv/Private/V8InspectorImpl.cpp")

if(ANDROID)
  add_library(${PLUGIN_NAME} SHARED ${SOURCE_FILES})
  install(TARGETS ${PLUGIN_NAME} DESTINATION ${PLUGIN_BINARY_FOLDER}/Android/${ANDROID_ABI})
elseif(IOS)
  set (CMAKE_IOS_INSTALL_UNIVERSAL_LIBS ON)
  set (CMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH OFF)
  set (CMAKE_XCODE_EFFECTIVE_PLATFORMS "-iphoneos;-iphonesimulator")
  add_library(${PLUGIN_NAME} STATIC ${SOURCE_FILES})
  install(TARGETS ${PLUGIN_NAME} DESTINATION ${PLUGIN_BINARY_FOLDER}/iOS)
elseif(WEBGL)
  set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} --em-config ${EM_CONFIG}")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} --em-config ${EM_CONFIG}")
  add_library(${PLUGIN_NAME} SHARED ${SOURCE_FILES})
  target_compile_definitions (${PLUGIN_NAME} PRIVATE WITHOUT_INSPECTOR)
  target_compile_definitions (${PLUGIN_NAME} PRIVATE WITH_QUICKJS)
  target_compile_definitions (${PLUGIN_NAME} PRIVATE PLATFORM_WEBGL)
  target_compile_definitions(${PLUGIN_NAME} PRIVATE
        _GNU_SOURCE
        CONFIG_BIGNUM
        CONFIG_VERSION="${QJS_VERSION_STR}"
        )
  set_target_properties(${PLUGIN_NAME} PROPERTIES PREFIX "" SUFFIX ".bc")
  install(TARGETS ${PLUGIN_NAME} DESTINATION ${PLUGIN_BINARY_FOLDER}/WebGL)
elseif(OSX)
  add_library(${PLUGIN_NAME} MODULE ${SOURCE_FILES})
  set_target_properties(${PLUGIN_NAME} PROPERTIES BUNDLE TRUE)
  target_compile_definitions (${PLUGIN_NAME} PRIVATE WITHOUT_INSPECTOR)
  target_compile_definitions (${PLUGIN_NAME} PRIVATE WITH_QUICKJS)
  target_compile_definitions (${PLUGIN_NAME}  PRIVATE PLATFORM_MAC)
  target_compile_definitions(${PLUGIN_NAME} PRIVATE
        _GNU_SOURCE
        CONFIG_BIGNUM
        CONFIG_VERSION="${QJS_VERSION_STR}"
        )
  install(TARGETS ${PLUGIN_NAME} DESTINATION ${PLUGIN_BINARY_FOLDER}/OSX)
elseif(WINDOWS)
  add_library(${PLUGIN_NAME} MODULE ${SOURCE_FILES})
  install(TARGETS ${PLUGIN_NAME} DESTINATION "${PLUGIN_BINARY_FOLDER}/Windows/${ARCH}")
elseif(UWP)
  add_library(${PLUGIN_NAME} MODULE ${SOURCE_FILES})
  install(TARGETS ${PLUGIN_NAME} DESTINATION "${PLUGIN_BINARY_FOLDER}/WSA/${ARCH}")
elseif(LINUX)
  add_library(${PLUGIN_NAME} MODULE ${SOURCE_FILES})
    if(ARCH STREQUAL "x86")
      set_target_properties(${PLUGIN_NAME} PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
    elseif(ARCH STREQUAL "x86_64")
      set_target_properties(${PLUGIN_NAME} PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
    endif()
  install(TARGETS ${PLUGIN_NAME} DESTINATION "${PLUGIN_BINARY_FOLDER}/Linux/${ARCH}")
else()
  message(FATAL_ERROR "Unsupported platform")
endif()
