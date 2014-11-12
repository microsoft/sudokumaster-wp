Sudokumaster Silverlight Example
================================

Sudokumaster is a Sudoku mobile game developed with Silverlight for Windows
Phone devices. The game is a logic-based, combinatorial number-placement 
puzzle with nine 3x3 grids each containing all the digits from 1 to 9. In the 
beginning only some of the numbers are placed in the grids and the player 
needs to figure out the correct positions for the missing numbers. 

The application is a rewrite of the Qt Sudokumaster application for Symbian 
and Maemo devices.


PREREQUISITIES
-------------------------------------------------------------------------------

- C# basics
- Development environment 'Microsoft Visual Studio 2010 Express for Windows
  Phone'

LINKS
-------------------------------------------------------------------------------

Getting Started Guide:
http://create.msdn.com/en-us/home/getting_started

Learn About Windows Phone 7 Development:
http://msdn.microsoft.com/fi-fi/ff380145

App Hub, develop for Windows Phone:
http://create.msdn.com


IMPORTANT FILES
-------------------------------------------------------------------------------

MainPage.xaml/.cs: Main page of the application, the game view.

HighscoresPage.xaml/.cs: High scores (or top times) page, contains a list of 
20 best times/moves.

Gamelogic.cs: Game board generation, logic for the game.

Cell.xaml/.cs: Represents a single cell on the game board.

GameOver.xaml: Dialog which is shown when the game ends.

NumberSelection.xaml/.cs: Dialog which is shown when the player taps on a cell.

WaitNote.xaml/.cs: Spinning circle animation which is displayed while
generating a new puzzle.


KNOWN ISSUES
-------------------------------------------------------------------------------

None.


BUILD & INSTALLATION INSTRUCTIONS
-------------------------------------------------------------------------------

**Preparations**


Make sure you have the following installed:
 * Windows 7
 * Microsoft Visual Studio 2010 Express for Windows Phone
 * The Windows Phone Software Development Kit (SDK) 7.1
   http://go.microsoft.com/?linkid=9772716


**Build on Microsoft Visual Studio**

Please refer to:
http://msdn.microsoft.com/en-us/library/ff928362.aspx


**Deploy to Windows Phone 7**

Please refer to:
http://msdn.microsoft.com/en-us/library/gg588378.aspx



RUNNING THE APPLICATION
-------------------------------------------------------------------------------

An empty Sudoku game board is displayed after the application is started. The 
menu at the bottom of the screen contains two buttons, New Game and Highscores.
Press the New Game button to start the game. Tap on an empty cell on the grid, 
and a dialog pops up where you can select a number or clear the value of the 
cell. Only the empty cells and the cells the player has set earlier (cells 
with white numbers) can be manipulated. The objective of the game is to fill
the board with numbers between 1 and 9 according to the following guidelines:
 
- A number can appear only once in each row
- A number can appear only once in each column
- A number can appear only once in each region

A region is 3x3 squares, and the board is divided into 3x3 regions identified
by lighter and darker cells.

Below the board are three icons and numbers besides them; number of moves the
player has made, remaining empty cells, and game time.

The game ends when all the cells are filled. If a new high score was achieved, 
the player's name is queried.


COMPATIBILITY
-------------------------------------------------------------------------------

- Windows Phone 7

Tested on: 
- Nokia Lumia 800
- Nokia Lumia 900

Developed with:
- Microsoft Visual Studio 2010 Express for Windows Phone


LICENCE
-------------------------------------------------------------------------------

You can find license details in Licence.txt file provided with this project
or online at
https://github.com/Microsoft/sudokumaster-wp/blob/master/Licence.txt


CHANGE HISTORY
-------------------------------------------------------------------------------

1.2 Code level improvements
1.1 Code quality improvements
1.0 First release
