Sapphire Biomolecular Imager capture software release notes.

What's New:

11/29/2023 (v1.3.3.1129)
===========================
Bug fixed: Cropping an image sometimes crashes the application with access violation error.

10/27/2023 (v1.3.2.1027)
===========================
Modified: New mainboard: uses the 'DevicePowerStatus' to check whether the ethernet connection is successfully connected. 

10/26/2023 (v1.3.2.1026)
===========================
Bug fixed: Error multiplex/merging EDR and non-EDR images.
Bug fixed: New mainboard: when the main power switch in the back is ON, and front power switch is OFF, and the PC's ethernet is connected to the SFL's ethernet, the capture hangs waiting for the motor to home. With the new mainboard, even though the front power switch is OFF, the capture software is still able to read the mainboard hardware ID and firmware version.
    WORK-AROUND: if not detecting any laser module - the capture software will consider it as an unsuccessful connection, because the new mainboard will say that ethernet connection is successful if the main power switch in the back is in the ON position.

10/26/2023 (v1.3.1.1026)
===========================
Modified: Don't allow 375 laser in R1 (Port #2)
Modified: Added a new flag 'IgnoreCompCoefficient' in SysSettings.xml to apply or ignore laser power compensation coefficient.
          Default is False (apply laser power compensation coefficient if set between 0 and 0.6), set to 'True' to ignore the laser power compensation coefficient.
          <IgnoreCompCoefficient Value="False" />

10/25/2023 (v1.3.1.1025)
===========================
Modified: Added laser power compensation coefficient.

10/23/2023 (v1.3.0.1023)
===========================
Bug fixed: Disable the 'Stop' scan button while preparing to scan a region (to avoid the error relating to saving the data of the uncompleted scan).
Modified: Added the 'Cancel' option to the image file too large message box to allow the user to cancel the scan.
Modified: Changed the 'Plate Holder' and 'Slider Holder' protocol scan regions (previous scan regions (ROI) was based on R&D beta unit).

10/20/2023 (v1.3.0.1020)
===========================
Bug fixed: EDR scan is not functioning correctly with multiple laser modules with the same wavelength.
Modified: Changed Auto-Alignment scan region coordinates, previous scan region (ROI) was based on R&D beta unit.

10/18/2023 (v1.3.0.1018)
===========================
Bug fixed: SmartScan is not functioning correctly with multiple laser modules with the same wavelength after the first scan.
           To reproduce: do a SmartScan (with multiple laser modules with the same wavelength), after the first SmartScan is completed, change the pixel size value then do another SmartScan; the SmartScan now only scanning one channel (when two channels were selected).

10/17/2023 (v1.3.0.1017)
===========================
Bug fixed: Auto-Alignment: Cannot do auto-alignment with multiple laser modules with the same wavelength (allow multiple laser modules with the same wavelength to auto-align).

10/16/2023 (v1.3.0.1016)
===========================
Bug fixed: New mainboard and firmware returns the value 65534 instead of 0 when a laser module is not mounted/installed on a laser module port.
Bug fixed: SmartScan is not functioning correctly when multiple laser modules with the same wavelength is selected to scan together.

10/13/2023 (v1.3.0.1013)
===========================
Bug fixed: When scanning with multiple laser modules with the laser type/wavelength, only one scanned channel image is saved sent to Gallery (removed the check if the same laser is selected multiple times).
Modified: Application Installer: removed quick launch option after the installation completed (the latest Windows Defender now detected that operation as potential harm and mark the installer as a virus).
 
10/12/2023 (v1.3.0.1012)
===========================
Modified: Merged hardware communication libraries from the ‘Avocado’ repository to the ‘Sapphire-FL’ repository for the new mainboard and firmware.

10/04/2023 (v1.2.8.1004)
===========================
Modified: Auto Align: Added options Settings to select the Edmund's target type (branding on top or branding on bottom).
          Auto-alignment scan region will be based on the selected Edmund's target type.

09/14/2023 (v1.2.7.0914)
===========================
Modified: Set the image metadata tag 305/Software to "Sapphire" instead of "Sapphire FL" so Azure Spot Pro could correctly open an Azure imager EDR image.
Modified: Save as JPEG/BMP/PUB TIFF restricted the DPI value to two decimal points when appending to the image file name.
Modified: Z-SCAN delta: restrict the entry value to 2 decimal places.

09/13/2023 (v1.2.7.0913)
===========================
Bug fixed: Saving an image as JPEG, BMP, and PUB TIFF updated the opened image's modified date (the opened was not saved; shouldn't update the modified date of the opened image in ImageInfo).
Modified: Create Single Channel: extracting RGB image to individual grayscale images doesn't allow an image with the same file name as an existing file in Gallery to be send to Gallery. Create Single Channel will now allow all extracted images/files to be send to Gallery. If an image/file with the same file name already exists/opened it will add a suffix (2), (3), etc. to the file name.

09/12/2023 (v1.2.7.0912)
===========================
Bug fixed: Add/copy a scan region does not copy the focus/sample type when is 'Custom' was selected.
Bug fixed: Don't check for the TIFF tag "PhotometricInterpretation" on non-TIFF file; could cause the application to crash opening a JPEG file.
Modified: Added default protocols "Slide Holder" and "Plate Holder" and make them always visible even if the laser modules in the protocols do not matched the current installed laser modules.
Modified: Don't allow the following default protocols to be deleted: 'New Protocol' 'Slide Holder' and 'Plate Holder' 
Modified: Only display the pixel as saturated if the pixel value is 255 on 8-bit images (JPEG, PUB TIFF, BMP). 

09/06/2023 (v1.2.7.0906)
===========================
Modified: Always save EDR image using 24-bit compression - set contrast display range from 0 to max (instead of using the full 24-bit display range, similar to ImageJ's scheme; they're using min to max contrast display range).
Modified: Always save EDR image using 24-bit compression - display the actual captured EDR bit depth instead of the compression bit depth in image info.

08/30/2023 (v1.2.7.0830)
===========================
Modified: Always compress and save EDR image as 24-bit.
          To revert back to the previous EDR implementation, set this value to false (default: true)
          <SaveEdrAs24bit Value="True" />

08/30/2023 (v1.2.6.0830)
===========================
Modified: Turn on auto-contrast by default when sending the scanned image to Gallery after the scan is completed.
Modified: Image Info: Despeckle Enabled (Yes/No) to Ignore Speckles (On/Off).

08/25/2023 (v1.2.6.0825)
===========================
Modified: Added options for select Chemi Module connection (Wi-Fi/LAN).
Modified: Launch Chemi Module: connect and open the Chemi Module home page based on the Chemi Module Connection type selected in Settings (Wi-Fi/LAN).
          URL for Wi-Fi and LAN connection:
            <ChemiModuleWiFiUrl Value="http://192.168.12.1/home" />
            <ChemiModuleLANUrl Value="http://192.168.1.40/home" />
Modified: Turn on saturation and auto-contrast by default when sending the scanned image to Gallery after the scan is completed.

08/23/2023 (v1.2.6.0823)
===========================
Bug fixed: Error saving as JPEG (bug introduced in the previous build (v1.5.6.0622)).

08/21/2023 (v1.2.6.0821)
===========================
Modified: Changed 'AUTO ALIGN' coordinate to correspond to the NEW Edmund Optics optical resolution card (branding on BOTTOM)
          <AutoAlign X="4.0" Y="1.5" Width="3.5" Height="1.5" />
Added: New user's manual in PDF format.
    The SFL capture will now:
        -Check if Adobe Reader is installed on the PC.
        -If Adobe Reader is installed on the system, the user's manual will be displayed in the application's help/user manual page.
        -If Adobe Reader is NOT installed, the application will try to open it with the web browser (first it will try to open the local copy of the user's manual (if you've installed the application a local copy will always be available locally); if the local copy is not found it will try to open the online copy of the user's manual via the web URL.

08/16/2023 (v1.2.6.0816)
===========================
Modified: Renamed Smartscan despeckler enabled/disabled checkbox from "Despeckle Smart Scan" to "Ignore Speckles"
Modified: Added 658 scan signal levels table in config.xml
Modified: Added a field in image info panel to indicate whether despeckler was enabled in 'SmartScan'
Modified: Move the field "Focus Position" below the "Focus Type" field in the image info panel
 
07/25/2023 (v1.2.6.0725)
===========================
Bug fixed: Error serializing the laser channel (LaserChannels) when a laser module is not installed in a laser module port (error saving the laser modules settings in the file 'Settings.xml' on application exit).
Modified: Custom Focus: modified the custom focus value out of range warning message (specified min and max focus value allowed).
Modified: Z-Scan Settings: restrict settings value to 2 decimal points.

05/12/2023 (v1.2.6.0622)
===========================
Modified: Check TIFF Tag PhotometricInterpretation, to properly open and interpret the chemiSOLO's chemi image file.

05/12/2023 (v1.2.5.0512) [Released]
===========================
Modified: Add Scan Region: copy auto-save settings when duplicating a scan region settings.
Modified: Phosphor Imaging: Add Scan Region: copy/duplicate the active scan region settings when adding a new scan region.
Modified: Allow the opened files to be closed from the Gallery's thumbnail image gallery panel.
            Display the close button when mousing over the thumbnail image.

05/05/2023 (v1.2.5.0505)
===========================
Bug fixed: Incorrectly assign the wrong color channel when scanning a single channel and assigned it to red/green/blue (image sent as grayscale in Gallery) (related to previous fix in v1.2.4.0427 large file size check and whether to auto-merge imaging channels).

04/28/2023 (v1.2.4.0428)
===========================
Bug fixed: Check file size:  when 'sequential scan' is selected file size was not checked whether the scan image will be too large to display in Windows (related to previous fix in v1.2.4.0427).

04/27/2023 (v1.2.4.0427)
===========================
Bug fixed: Vertical bar on the scanned image; the application didn't allocate enough buffer size to compensate for the horizontal alignment. 
           (May see a vertical bar on the image if horizontal alignment offset value (L_dx or R1_dx) is large on a system)
Bug fixed: Check file size and display warning before the scan starts. Windows has a problem displaying an image with the size greater 2GB, if a scan region will generate an image greater 2GB, the scan channels will not automatic merge (each channel will be display and save as an individual channel).

04/25/2023 (v1.2.3.0425)
===========================
Bug fixed: Read the laser temperature using 2 bytes (previous using 4 bytes). The 'new' laser module now store the laser temperature in 2 bytes.
Modified: When EDR scan failed to find the scale factor, send the smart scan image (reference image 1) to Gallery instead of the saturated image (reference image 2).

04/20/2023 (v1.2.3.0420)
===========================
Bug fixed: Z-Scan + EDR scanning throw an exception.
           Added handling of Z-Scan with EDR.
Bug fixed: SmartScan's preview image not displaying on Z-Scan + SmartScan (after the first Z, but non-SmartScan preview still displaying).

04/14/2023 (v1.2.3.0414)
===========================
Bug fixed: Don't prompt to save the data when aborting EDR scan.
Modified: changed line average calculation.

04/13/2023 (v1.2.3.0413)
===========================
Bug fixed: The scanner's LED progress bar not synching with the estimated scan time (when line average is on/enabled).
Bug fixed: The focus position not displaying when the scanned image is sent to Gallery right after a scan is completed.

04/12/2023 (v1.2.2.0412)
===========================
Modified: added error checking in EDR scanning.

04/10/2023 (v1.2.2.0410)
===========================
Modified: New EDR scanning algorithm
Added: Settings: added option to do despeckle on Smart Scan's test image(s) to remove hot spots (speckles of dirt/dust/lints/etc).
                 Default: OFF

04/05/2023 (v1.2.1.0405)
===========================
Modified: Scale bar: don't set default scale bar width to 1-cm/1-inch if the image is less than 1-cm/1-inch.
Modified: Scale bar: when saved as TIFF PUB or JPEG, don't set the image to display the scale bar when opened in the capture software; the scale bar is already written on the image for these types of images, otherwise they will be overlapped.

04/04/2023 (v1.2.1.0404)
===========================
Modified: added 450 signal levels in config.xml

04/03/2023 (v1.2.1.0403)
===========================
Modified: Add 375 and 450 to smart scan PMT signal level calculation (using the same formula as 488/PMT)
Modified: Auto Align: update to initial message upon starting auto align (GH#84)
Modified: Auto Align does not stop you when there is only one module loaded (GH#101)
          Prompt to load another laser module (give the option to home the scan head and close the application [go through 'Change Laser Module' process).
Modified: Disable "Highest" Quality for 5um scans (GH#111)

03/30/2023 (v1.2.0.0330)
===========================
-Added: When lid open is detected, the application will terminated the scan and prompt to save the scan data.
-Modified: Update Phosphor Imaging signal levels gain value in config.xml.
-Bug fixed: Phosphor Imaging: application throws an exception when the user stop the scan and select 'Yes' when prompted to save scanned data (application crashes).
-Bug fixed: Fluorescence Imaging: Z-Scan: No data/image is saved when the user stop the scan and select 'Yes' when prompted to save scanned data.
-Bug fixed: When multiple laser modules with the same wavelength (i.e., 638/APD and 638/PMT) is loaded, the saved protocols is incorrectly loaded (incorrectly removed the protocols from the list for not having matching lasers).

03/28/2023 (v1.2.0.0328)
===========================
Modified: SmartScan: multi-channel scan: when all scan channels saturated, stop the test scan (and do the calculation or predict the new signal level).
                     multi-channel scan: when all channels are not saturated, don't turn off the laser on the saturated channel(s) to avoid the confusion in the preview image. Previously if you scan 3 channels and 2 channels saturated, it will turn off the lasers for the 2 saturated channels and the scan preview image will only continue to display the non-saturated channel.
                     When checking the saturated pixel(s) exclude the overscan rows and columns (we scan extra rows/lines to get rid of the blank/empty bar when the images are aligned, we also scan extra columns because of the laser heads offset).
Modified: Auto Align: Removed the added x overscan buffer.

03/24/2023 (v1.2.0.0324)
===========================
Modified: SmartScan: for 50 micron scan and below, do smart test scans at 100 micron.
Modified: SmartScan: scan the whole selection region, don't turn off the laser when detecting a saturated pixel (avoid the confusion in the preview image, but will take longer to do smart scans).
Modified: Allow Imaging panel to scroll while scanning (GH#107).
Modified: Disable 'CHANGE LASER MODULES' button while scanning.

03/23/2023 (v1.2.0.0323)
===========================
Modified: Gallery -> Resize: Allow image pixel dimensions resize.

03/21/2023 (v1.2.0.0321)
===========================
Bug fixed: Removed the overscan in the X direction.
  - previous build move the scan head too far on full width scan.
  - may see blank bar on the side on resolution (small pixel size) scan.

03/20/2023 (v1.2.0.0320)
===========================
Modified: Removed the special case that was added for 150 micron scan, which causes the jagged edges (sawtooth) in the 150 micron scan in the standard/user UI scan.
Bug fixed: Create Multiplex: allow single channel grayscale multiplexing into Red, Green, or Blue channel (grayscale into the gray channel not allowed).
 
03/19/2023 (v1.2.0.0319)
===========================
Added: Arbitrary rotation window: improve arbitrary rotation on large image (let WPF (.NET) process/handle the image rotation in the rotation preview window (WPF is more efficient processing the image rotation, especially on a very image). 
Bug fixed: Create Multiplex: RGB and grayscale image merge not allow (currently not supported) (GH#103).
Bug fixed: Create Multiplex: Allow a single channel to merge into an RGB (GH#105).

03/13/2023 (v1.2.0.0313)
===========================
Bug fixed: Error creating a multiplex image with EDR. 
Modified: Auto Align: allows auto alignment with Phosphor module installed (if there are laser modules in R1 and R2).
Modified: Auto Align: allows auto alignment with laser modules in Port #1 (L) and Port #2 (R1).
NOTE: possible auto alignment combinations: L+R1+R2, R1+R2, and L+R1

03/10/2023 (v1.2.0.0310)
===========================
Modified: Added overscan width and height for 'AutoAlign' scan (for the new laser modules image alignment method).

03/09/2023 (v1.2.0.0309)
===========================
Modified: Laser modules image alignment: new image alignment method - no longer shifting the image up/down or side to side to avoid the blank edges (the size of the blank edges depends on the laser modules alignment). In order to align the image channels without shifting the images; we're now adding a 0.6mm vertical overscan (which is equivalent to 60 pixels (or added a 30 seconds scan time) for a 10 micron scan. In a low resolution (high pixel size) scan, you're probably not going to notice the added extra scan time.
The overscan value is defined in config.xml
    <ScanningSetting Name="YMotionExtraMoveLength" Value="0.6"/> 
NOTE: with the added overscan the first 15 seconds of a 10 micron scan; you'll not see anything in preview because it's doing the first half of the overscan (outside of the user selected scan region).

Bug fixed: Preview Image Contrast: Click on the 'Auto' button doesn't always executed the auto-contrast calculation (especially on a system with fast processing speed).
Bug fixed: Create Multiplex: catch the exception and display the error message instead of re-throw the exception to avoid crash the application.

03/06/2023 (v1.1.0.0306)
===========================
Modified: Phosphor Imaging: default to software only line averaging instead of a combination of hardware + software line averaging scheme.
          Turn off hardware + software line average in config.xml: 
              <ScannSetting Name="PhosphorModuleProcessing" Value="False"/>

03/01/2023 (v1.1.0.0301)
===========================
Bug fixed: SmartScan: Smart + simultaneous - if signal level already found/calculated for a channel; don't recalculate the signal level for that channel on the subsequent 'Smart' scans.
Modified: 'Auto Align' send the aligned image Gallery even when the laser modules appeared to be already aligned.

02/28/2023 (v1.1.0.0228)
===========================
Bug fixed: Auto Align algorithm sometimes failed to find the alignment offset/parameters on the scanned images.
Bug fixed: Disable 'Save', 'Save As', and 'Save All' while busy saving a file (to prevent the user from select a 'Save' file it's busy saving a file (G#50)).
Added: Auto align scan region/coordinate to config.xml instead of using hardcoded values.
Modified: SmartScan will not turn off the 685 and 784 laser when detected saturated pixels (685/784 picks up dust/lint's and return a lower signal level than expected - the low resolution test scan(s) will now scan the whole selected region and apply mean filter to try to remove the hot pixels before calculating the signal level).
Modified: Set scan type to EDR when EDR scan is selected.
Modified: Added a flag to save the Auto Align scanned images for troubleshooting purposes (default: ON/enabled).
Modified: Send the auto aligned image to Gallery after Auto Align is completed.
          This option is controlled by the flag in config.xml (default: ON/enabled)
          <SendAutoAlignedImageToGallery Value="true" />
Modified: Update message box phrasing (GH#81/#82/#84).

02/21/2023 (v1.1.0.0221)
===========================
Bug fixed: Error scanning 'Auto Align' with 2 laser modules. Allows aligning a system with only 2 laser modules (L + R2 or R1 + R2).
Bug fixed: Abort the EDR scan when it failed to calculate the scale factor (instead of continue to scan until it reach scan level 10 (highest scan level). {abort the EDR scan after 2 failures).
Bug fixed: Gallery: Resize tab's controls are disabled (or not the top most panel).
Modified: Updated message text: Auto Align: a laser module is not in Port 3 (GH#79).
Modified: Updated message text: Auto Align: phosphor module is detected (GH#81)
Modified: Updated message text: Phosphor laser module in an incompatible port (GH#82).

02/16/2023 (v1.1.0.0216)
===========================
Added: 'Auto Align' to get and set the image alignment parameters on the scanner (it's going to assume the user has placed an Edmund resolution target at the bottom left corner (toward the front of the scanner) - it will scan a small region at 10 micron to do the alignment calculation.
Added: Imaging processing library 'OpenCV' (needed for Auto Alignment' feature).
Bug fixed: Create Multiplex: multiplexing/merging EDR images changed the source during conversion.
Modified: Changed application .NET Framework target (v4.6.1 to v4.8) [the new imaging library required .NET Framework v4.8]
Modified: Disable the 'SmartScan' button when EDR scan is selected.
Modified: Clear the preview image when all the display channels are deselected (GH#14).
Modified: Hide the 'true' DynamicBit bit depth (EDR field in image info will display 18-bit will be display the EDR value of 2, 19-bit displays the value of 3, etc.) (GH#71).
Modified: Quality in image info now refers to whether 2-line average was ON/OFF.
          Highest = ON, High = OFF (previously Quality displayed the scan speed value).
Modified: Scale bar: changed the default unit of length to cm.

02/02/2023 (v1.0.0.0202)
===========================
Bug fixed: File saving access error after the image is modified (GH#50).

02/01/2023 (v1.0.0.0201)
===========================
-Modified: When merging images (or creating multiplex), if the DynamicBit of the merging images are different, convert the image with the lower DynamicBit to the highest DynamicBit of the merging set of images.
-Modified: EDR scan - applying median filter on the test/calculating image to remove hot pixels (if there are dusts or lint on the glass, it may cause the SmartScan to return a lower scan signal level (especially the IR lasers). If the  scale factor is greater than 10, move to the lower scan signal level to avoid over-saturating (which cause a bloom around the band, which would cause a dark ring around the saturated bands after EDR processing).
-Modified: When 'Highest' Quality is selected (line average ON), turn off line average on the test scans of SmartScan and EDR scan (to not slow down the scan process).
-Modified: Custom Focus - set the range of the focus slider based on the scanner focus position
           (i.e., if the scanner focus position is at 1.2, the Custom Focus range allows will be from -1.2 (below top of glass) to +5.8 (above the top of glass)
-Modified: Z-Scan - restrict the value enter within the allowable limit.
-Modified: Prompt for the password when launching the EUI application.

01/30/2023 (v1.0.0.0130)
===========================
Modified: Removed scan 'Quality' option from Phosphor Imaging.
Modified: Removed scan head locking option from the 'Advanced' tab in 'Settings.'
Modified: Changed scan 'Quality' option labels (Medium/High) to (High/Highest).

01/25/2023 (v1.0.0.0125)
===========================
Modified: Updated default emission filters (SysSettings.xml).
          Changed Bandpass variable type from int to string (bandpass value can have non-integer values (see Phosphor Imaging's emission filter).
Bug fixed: Laser and Filter Pairing crashes application when previous laser module pairing doesn't find matching in the current laser and filter options list (GH#60).

01/24/2023 (v1.0.0.0124)
===========================
Bug fixed: Validate the file name entered when auto-save is enabled.
Bug fixed: Terminate the EDR scan when Smartscan (first phase of EDR scan) is saturated. 
Modified: Terminate the scan if it takes longer 15 seconds to retrieve a line of data from the scanner.

01/23/2023 (v1.0.0.0123)
===========================
Bug fixed: Phosphor Imaging's auto-save file saving throw an exception (crashes the application).

01/20/2023 (v1.0.0.0120)
===========================
Bug fixed: Phosphor not showing when Phosphor module is installed/mounted.

01/19/2023 (v1.0.0.0119)
===========================
Added: 'Scan Quality' options (Medium/High) in scan control in Fluorescent and Phosphor imaging tab.
       'High' (corresponds to Sawtooth OFF, 2-Line Averaging ON) and 'Medium' (corresponds to Sawtooth ON, 2-Line Averaging OFF, and is the default setting in Fluorescent). 
Bug fixed: Trying to save a modified images that was flipped vertically/horizontally, sometimes throw a file access exception error.
Modified: Removed "Export to AzureSpot" from the file menu
Modified: Removed 'Sawtooth correction' and '2-line average' option in Settings (previous added to for testing the feature purposes).

01/13/2023 (v1.0.0.0113)
===========================
Bug fixed: Channel pixel alignment: shift up throw an exception when the width of the image is an odd value. 
Bug fixed: When 780 laser is detected- rename the 784 signal level lookup table key to 780 (allow backward compatibility for the old 780 laser module)
Modified: Reverted back the step size of the X and Y motor.

01/11/2023 (v1.0.0.0111)
===========================
Modified: Automatically try to connect to the Chemi Module (if not already connected) when 'Launch Chemi Module' button is selected.
          IMPORTANT: the name of the Chemi Module's SSID (WiFi access point) must start with "SFL"
Modified: Changed error messages (GH#45, GH#54).
Modified: Removed the work-around when detected a 780 laser (will not renamed it as 784).
Modified: Use different icons for the manual channel alignment.
Modified: UI change - changed button style (changed the look and behavior of MouseOver and button pressed).

01/09/2023 (v1.0.0.0109)
===========================
Added: Manual image pixel alignment (shift: rows- up/down, columns- left/right).

01/06/2023 (v1.0.0.0106)
===========================
Bug fixed: Scan region rect/selector should not be able to drag outside of the scan region grid.
Modified: Changed the X and Y motor step size in the config.xml file (XMotorSubdivision/YMotorSubdivision) [the previous step size were too small]

12/29/2022 (v1.0.0.1229)
===========================
Modified: Updated imaging controls UI.
Modified: Updated Splash screen image (no copyright text).
Modified: Apply changed property to LaserModule properties (to trigger an update on detected laser modules).
Modified: Add a short delay (1 second) and re-try getting device properties (when first attempt failed).

12/28/2022 (v1.0.0.1228)
===========================
Modified: Reset the preview image alignment and image updating flags when allocating the preview image buffers.
Modified: Set the ComboBox height in the signal control (Pixel Size/Scan Speed/Focus) in Fluorescent/Phosphor; laser module filter pairing dialog box to prevent them from getting smaller when these controls are disabled.
Modified: Z-Scan: don't allow focus delta below 0.01mm.
Modified: Set Custom focus slider's small step size to 0.01.

12/27/2022 (v1.0.0.1227)
===========================
Modified: Reduced CPU usage (idle & busy).
          Disable SOCKET receiving thread and change to event receiving.

12/22/2022 (v1.0.0.1222)
===========================
-Modified: 5 micron scan: scan at 10 micron at high speed (instead of highest), but if the user selected a scan speed below high (medium/low), use the user's selected speed.
-Modified: Allow the user to open images at one time. (Allow the user to select multiple to images to open in the open file dialog).
-Modified: Z-Stacking: popped up a message if the selected image is not 16-/8-bit image.
-Modified: Z-Stacking: When a selected item is added (and removed from the images list), select the next image (first image on the list).
-Modified: Added more logging to try to catch preview image sometimes stop updating.

12/20/2022 (v1.0.0.1220)
===========================
Modified: Enable laser temperature logging by default. Set default logging interval to 30 seconds.
          <LaserTempLogging Enable="true" Internal="30" />
Modified: Don't apply sawtooth correction if 'XOddNumberedLine' and 'XEvenNumberedLine' are both set to 0.
Bug fixed: Scan ROI was able to drag outside of the grid (bug introduced in the previous build).

12/19/2022 (v1.0.0.1219)
===========================
Added: User's UI: added lasers' sensor temperature logging.
       To enable temperature logging, set LaserTempLogging value to "true"
         <LaserTempLogging Value="true" />
         Log file name: Temperature.log
Modified: 'Image Alignment Settings' parameters in the Settings tab now expecting the user to scan at 10um (previously to speed up the alignment process it expected the user to scan at 20um and then converted the parameters entered to 10um values (param x 2)).
 
12/16/2022 (v1.0.0.1216)
===========================
Added: "Launch Chemi Module" button to Navigation bar (if set to be visibled in SysSettings.xml).
         Enable Chemi module set ChemiModule value to 'True'
           <ChemiModule Value="True" />
         To specify a different URL (change 'ChemiModuleURL' value)
           <ChemiModuleURL Value="http://192.168.12.1/home" />
Modified: Try to reduce CPU usage:
            Changed QueryThread from System.Threading to to System.Timers.Timer in MotionLib library
            Changed Thread.Sleep(10) to Thread.Sleep(1) in AbsoluteMoveSingleMotion() because it's in a loop (for context switching purpose).
            Changed ShowRedTemperatureTimer from System.Threading to to System.Timers.Timer in EUI

12/14/2022 (v1.0.0.1214)
===========================
Modified: Changed the default 'MotorPolarities' value to the settings required for the newly received scanners.
Modified: Preview image update: don't immediately update the preview image while scanning when the Black/White/Gamma value or Auto is clicked (only set the contrast values) and let the automatic preview image update do the updating when it received a new row of data update with the contrast values (to avoid manually update and automatic update conflict (on a system with a slower processor this could be an issue)).
Modified: Scan region ROI uses 2 decimals places value for more precision.
Modified: Phosphor Imaging: sort the protocols and the default protocol, 'Phosphor Imaging' the first item. (to make it consistent with the Fluorescence imaging).

12/08/2022 (v1.0.0.1208)
===========================
Bug fixed: The scan process uses 100% CPU on the DELL laptop with the I5 processor. 
Modified: Updated Phosphor Imaging scan level intensity settings (lowered/decreased laser intensity).

12/06/2022 (v1.0.0.1206)
===========================
Added: Z-Stack (Z-Projection) feature.

12/02/2022 (v1.0.0.1202)
===========================
Modified: changed contrast buttons (R/G/B) to match contrast buttons in Imaging. (GH#47)
Modified: changed text 'sample type' to focus type (Settings tab/ImageInfo/message boxes). (GH#46)
Modified: Always launch the application in full screen (maximized) mode.

11/30/2022 (v1.0.0.1130)
===========================
Bug fixed: Phosphor Imaging scan error (error setting up the preview image buffer)[introduced in the previous build].
Modified: When adding a new scan region, copy/duplicate the settings from the selected scan region. (GH#11)
Modified: SmartScan: clear the preview image after each test image scan.
Modified: Changed the EDR and Sequential scanning toggle button styling (uses ToggleButton styling instead of using the code behind to change the icons; didn't behave as expected in certain scenario).
Modified: EUI: save all ".csv" files in ProgramData (instead of the CWD).

11/29/2022 (v1.0.0.1129)
===========================
Modified: Hide the arrow down icon and change the text to DarkGray on the Intensity dropdown menu when EDR scan is selected. (GH#20)
Modified: Sort the protocols and move the 'New Protocol' item to the top of the list (and selected by default on application load). (GH#42)

11/28/2022 (v1.0.0.1128)
===========================
Bug fixed: SmartScan + Sequential error calculating the signal level for Port #3/(R2)(error introduced in the previous build).
Bug fixed: Check if a laser is mounted/installed before verifying whether the laser module is in a compatible port.
Modified: Z-Scan: Clear the preview image after each scan layer is completed.
Added: more loggings to try to isolate why/where the scan stopped (on a Sequential scan, the scan sometimes stop when trying to scan the next channel, after the first channel is completed).

11/23/2022 (v1.0.0.1123)
===========================
Bug fixed: SmartScan: calculated/returned the incorrect signal level.
           When detected saturation - scan 5 rows before turning off the laser to make it more obvious when the laser was turned off during SmartScan.
Modified: Optimizing SmartScan, EDR scan and the combination of EDR+Sequential and Sequential+SmartScan (avoid scanning a channel already has an optimal signal level) (GH#39).
Modified: Updated the default/standard filter list (GH#37).
Modified: Display 780 laser module as 784; if/when the laser module is detected as 780, display/label as a 784 (GH#41).
Modified: Remove the 'Cancel' button from laser module and filter pairing window (GH#43).
Modified: Update of error message required (module in wrong port) (GH#45).

11/18/2022 (v1.0.0.1118)
===========================
Modified: Display a warning message when the Phosphor module is not mounted/installed in Port/Slot #1.
Modified: Display a warning message when the laser module is not mounted/installed on the proper port/slot number (GH#36).
Modified: Always show laser/filter info (previously only show when in imaging mode) (GH#15).
Modified: Greyed out (or disabled) the intensity dropdown when EDR scanning is enabled (GH#20)
Modified: Scan region create via dragging the corners and manually entered same scan area settings do not align (GH#32). [Floating rounding issue, now rounding to 1 decimal point (or nearest 1mm) like the UI display]
Modified: Smartscan: save test images (when enabled in config.xml).

11/16/2022 (v1.0.0.1116)
===========================
Added: Sawtooth correction and 2-line average option to the Settings tab (these options override the same flag in 'config.xml' file).
Bug fixed: Multi-region scan: removing a scan region after a scan is completed crashes the application. 
Modified: Setup: install the EUI executables along with the user's UI (EUI now uses the same 'config.xml' as user's UI and from the same ProgramData folder 'Sapphire FL').
Modified: EUI: Create 'ImageGif' and 'Log' folder in 'ProgramData' (can't create these folders in Program Files' (when the app run from the installation folder)).

11/14/2022 (v1.0.0.1114)
===========================
Bug fixed: 5 micron scan uses incorrect width and height (bug introduced in the previous builds)
Modified: Save raw image(s) when doing Z scanning (when save raw image is enabled).

11/11/2022 (v1.0.0.1111)
===========================
Bug fixed: Sawtooth correction: applied sawtooth correction before applying image alignment.
           Set sawtooth correction value 'XOddNumberedLine' to -1

11/10/2022 (v1.0.0.1110)
===========================
Modified: use Microsoft image type (WriteableBitmap) directly instead of converting ushort array to WriteableBitmap.

11/09/2022 (v1.0.0.1109)
===========================
Modified: Set sawtooth correction value 'XOddNumberedLine' to -1
Modified: Allow the user to set the display image rendering option in the configuration file (default: <BitmapScalingMode Value="HighQuality" />)

11/08/2022 (v1.0.0.1108)
===========================
Bug fixed: Another attempt at sawtooth correction (correct floating point calculation rounding issue - now truncating instead of rounding).

11/07/2022 (v1.0.0.1107)
===========================
Bug fixed: Sawtooth correction (correct floating point calculation rounding issue).

11/03/2022 (v1.0.0.1103)
===========================
Bug fixed: When the width of the image is made an even value to avoid the image skewed, delta X value was not updated to reflect the change in image's width.
Modified: Save Protocol: Save destination file location.
Modified: Generate new file name after a scan is successfully completed (to avoid overwrite previous scanned image)
          - If the file name is in Azure default file pattern (date_timestamp), generate a new file name.
          - if the file name is a user specified file name (doesn't have Azure's default name pattern), add the suffix (number inside a parenthesis (i.e., FileName (2).tif)).
Modified: Add more loggings.

10/28/2022 (v1.0.0.1028)
===========================
Modified: Sawtooth correction: changed default value for "XOddNumberedLine" to -2 for 10 micron scan and -1 for scan resolution above 10 micron
Modified: Added a status message "Downloading" when processing the scanned image to indicate that it's busy doing something and not appear as if the application hang.

10/27/2022 (v1.0.0.1027)
===========================
Bug fixed: Fixes sawtooth issue (since the changes will cause the image to skewed when the width of the image is an odd value (like the EUI); as a work-around we'll always make the width an even value to avoid the skewed scan image).

10/26/2022 (v1.0.0.1026)
===========================
Added: 'PixelOffsetProcessingRes' to specify which scan resolution to apply the sawtooth correction (default: below 50 micron scan resolution) 
Added: 'KeepRawImages' flag in config.xml to allow saving the raw image(s) for debugging purposes [ON: send raw image to Gallery].
Bug fixed: Full width (25cm) scan hang the application after the scan job is completed.
Bug fixed: SmartScan: may returns an incorrect signal intensity level (especially if there's multiple samples (with different brightness) and all the samples are not all samples are selected in the scan ROI) (previously using the raw image which included large overscan area outside of the selected scan region to find the max pixel; will now use the cropped (or selected) region to find the max pixel)
Bug fixed: Focus position incorrectly set when selecting the default focus value (Gel (+0.50)/Plate (+3.00)/Membrane (0)/Slide (+1.00))
           [Custom and Z-Scan focus setting was properly set]           
           [SFL add to move to up and SOG minus to move up]
Bug fixed: Application hang when there are duplicate laser modules installed (not likely to occur in the real world).
Modified: New signal level parameter changes for 532/638/685/780.
Modified: Uses new icons for the laser ports

10/14/2022 (v1.0.0.1014)
===========================
Bug fixed: EDR + Sequential scan cause the application to hang.
Bug fixed: Incorrect PGA value send to the scanner (fix in config.xml signal level parameter)
Bug fixed: Save All - added an extra (.tif) extension to a file name that already has a .tif extension.
Bug fixed: Preview image contrast control's White value step size is too small (clicking on the slider bar does not appear to move the slider because the step size is too small)
Modified: Enable 'sawtooth' correction by default: <ScannSetting Name="PixelOffsetProcessing" Value="True"/>

10/07/2022 (v1.0.0.1007)
===========================
Bug fixed: EDR scan hangs while processing EDR image.
Bug fixed: EDR scan: incorrectly display image info on EDR scanned image.
Bug fixed: Laser/filter dropdown not updated after laser and filter pairing.
Modified: EDR scan: add more logging and correct the incorrect logging
Modified: EDR scan: display the scan time countdown when scanning the final image.
Modified: EDR scan: Turn off saturation display while scanning EDR.
Added: option to always display/pop up the Laser and Filter pairing window on the application launch (turn on/off in the Settings tab - off by default).

10/03/2022 (v1.0.0.1003)
===========================
Bug fixed: Added missing 2-lines average scanning setup (double the size of DY (height)

09/28/2022 (v1.0.0.0928)
===========================
Added: 2-lines average in user/standard UI capture software.
       Turn on 2-line average by default for Phosphor Imaging.
           AllModuleProcessing        (Enable/disable Fluorescence 2-line average)
           PhosphorModuleProcessing   (Enable/disable Phosphor imaging 2-line average)

09/27/2022 (v1.0.0.0927)
===========================
Added: Control/window:
       - Change Laser Module control/window
       - Laser and Filter pairing control/window
       - Laser Module info on main navigation bar (when the Imaging is selected)
       - Add Filter control (in Settings/General tab)
Added: Image Info: filter field
Modified: Focus and Filter listview - changed MouseOver and selected item background
Modified: Remove the preview image when the scan region is deleted.
Modified: Remove the Phosphor laser module from Fluorescence Imaging list of available/installed laser modules.

09/12/2022 (v1.0.0.0912)
===========================
-Bug fixed: Incorrectly referencing the wrong view model.
-Bug fixed: Turn off EDR scanning when doing preview scan.
-Modified: Don't create the 'Masters' folder.
-Modified: IsFluorescenceImagingVisible public properties (RaisePropertyChanged).

09/07/2022 (v1.0.0.0907)
===========================
Bug Fixed: Error loading the protocol (occurred when the protocol included the laser that's not mounted/installed).

08/31/2022 (v1.0.0.0831)
===========================
Added: option to add/edit image info's DynamicBit (default: hidden, display: ctrl + right click on ADD/EDIT button).
Settings tab/page UI updates.

08/24/2022 (v1.0.0.0824)
===========================
-Added EDR scan
-Modified: Updated UI: Preview/Contrast window.
-Modified: Laser signal changed for each laser type (see Wei).

08/10/2022 (v1.0.0.0810)
===========================
-Bug Fixed: Z-Scan: Incorrect label of the focus value in the image/file name. 
-Bug Fixed: Custom focus: the custom focus moved in the wrong direction.
-Added: GIF animation: Added quality options for animated GIF creator.

08/09/2022 (v1.0.0.0809)
===========================
-Bug Fixed: Z-Scan: Z motor went in the wrong direction (the original Sapphire home is close to the glass and the Sapphire FL home is away from the glass).
-Modified: Implemented save & delete protocol (added DeleteProtocol databinding (new window/dialog box)).

08/08/2022 (v1.0.0.0808)
===========================
-Bug fixed: Z-Scan settings (allow Z range of -2 to + 7)
-Modified: ROI page - re-arrange the icons to Vicky's design.
-Modified: Don't clear the preview image(s) after a scan is completed.

08/05/2022 (v1.0.0.0805)
===========================
-Bug Fixed: Smartscan: signal level not correctly saved.
-Modified: Increase the zoom level for the preview image/window.
-Modified: Allowed the scan region selection box to be change while scanning for a single region scan.
-Modified: Display the image channel contrast checkbox on multi-channel scan (to allow an individual channel contrasting in preview).

08/02/2022 (v1.0.0.0802)
===========================
Added: new image info icon for Gallery's navigation bar.
Bug Fixed: 5um scan image resize error (image not resized).
Bug Fixed: Smartscan: scanning preview image not displaying for the final scan of the Smartscan (if selected resolution is not 500).
Modified: Smartscan: Don't the display the time remaining countdown while Smartscan is calculating the signal level (display the countdown only on the final scan, so not to confuse the Smartscan test scans with the final scan).

07/29/2022 (v1.0.0.0729)
===========================
Bug Fixed: Scanning preview image(s) not aligned.
Added: laser channel slot (L1/R1/R2) in image info (for debugging purposes).

07/27/2022 (v1.0.0.0727)
===========================
-Added: 5um resolution option (scan at 10um at speed of 2, then resize to 5um resolution).
-Added: Home scan head button to Settings tab (to allow the user to home the scan head/stage so that they can swap the laser modules).
-Bug Fixed: multi-region scan error.
-Bug Fixed: high resolution hangs the application in the previous build (50um and below) [now align the preview image(s) in a separate thread].
-Modified: Sequential scan: De-select the completed scanned channel.
-Modified: Preview scan: don't clear the preview image(s) on scan region settings changes.
-Modified: Gallery: used the correct icons for the buttons in ROI page.
-Modified: Phosphor Imaging: align the preview image(s) in a separate thread (to avoid locking up the main thread).
-Modified: Read and save the system serial number from the firmware.
-Modified: Update auto-save file location control to dark color theme.
-Modified: Update Phosphor imaging panel to dark color theme.
-Modified: Update signal data template to dark color theme.
 
07/22/2022 (v1.0.0.0722)
===========================
-Bug Fixed: The PMT gain is not set when scanning on a laser module with the PMT.
-Modified: SmartScan: turn off laser when detected saturation.
-Modified: Detect a phosphor module and add Phosphor Imaging tab - if the laser is 638/658/685 and its sensor is a PMT then it's considered a phosphor module.
-Modified: Display a message when there's no lasers detected (and switch to the Gallery tab)
-Modified: new splash screen image.
-Added: image alignment parameters setting in the Settings tab.

07/20/2022 (v1.0.0.0720)
===========================
-Bug Fixed: scanned image skewing.
-Bug Fixed: Gallery: Saturation command not function (new button design missing command binding). 
-Bug Fixed: Gallery: Color channel displaying and contrast not function correctly (new button design implementation changed the behavior of the buttons)
-Added: Enable Extended Dynamic Range in Settings tab (this option override the flag 'ScanDynamicBitAt' in config.xml)
-Added: ROI gallery tab (Copy/Copy/Paste) [still need the correct button icons, currently using existing icons as placeholder).
-Added: Image Info gallery tab [still need the correct buttons icon]
-Added: Image Orientation: arbitrary rotate (free rotate)
-Modified: changed Gallery tab navigation buttons from ToggleButton to RadioButton behavior to make sure that only 1 button is selected at a time, and reduced the amount of flags to keep track of, and remove the code behind). 
-Modified: Image Orientation: changed Rotate (arbitrary rotate) to a ToggleButton.
-Modified: removed button/menu item from the main Gallery menu/tool bar: Select, Copy, Paste, Info
-Modified: Scale bar: set default selected color to 'White'
-Modified: Changed EULA text (Sapphire to Sapphire FL)

07/17/2022 (v1.0.0.0717)
===========================
-Initial Sapphire FL capture software build.