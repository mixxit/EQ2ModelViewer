@echo off
for /F "tokens=*" %%A in  ( zones.txt) do  (
   ECHO Processing %%A.... 
   CALL EQ2ModelViewer.exe norender export %%A
)
@echo on