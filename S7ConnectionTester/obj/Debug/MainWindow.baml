   M S B A M L   `   `   `  ����M  IS7ConnectionTester, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:S7ConnectionTesterS7ConnectionTester  5   	   #    S7ConnectionTester.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:S7ConnectionTester     5   	     � Title$  PLC Communications��5   	   
����İ�$��Times New Roman)�6@    � WindowStartupLocation$ CenterScreen=�6]   $��14(�6�   $	��600��6�   $	��770��6�    � Icon$ Resources\osa_ics_plc.ico��6�   $	��618��6�   $	��922��6�   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��222* �6   �� 5      $
��299* �6   �� 5      $
��394* �6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��208* �6   �� 5      $	��74* �6   �� 5      $
��179* �6   .+�5   
   � $��5,5,5,8q�6   $��Stretch=�6    $��1��6<   $��0��6I   $��2��6Y   .+�5      �� 
����  �$��2q�6(    �� CornerRadius$ 20��6<    �� Padding$ 10q�6N   ._�5      �� .+�5      �� $��
1.Plc data��6   $��Center=�61   $��Comic Sans MS)�6N   $
��Bold"�6i   $��15(�6{   �� 5      $��PLC processor type:��6   $��Center=�6:   $	��Top=�6W   �� 5      -   5         ��Name$ CpuTypeComboBox��5      $��Stretch=�68    �� DisplayMemberPath$	 Key��6v   	 �� SelectedValuePath$	 Value��6�   ��6V   �� *	cpuType+6�   �� 5      $��IP address:��6   $��Center=�62   �� 5      -   5        
 ��Name$
 	ipTextBox��5      $��Stretch=�61   
������� �� TextAlignment$ Center=�6�   ��6g   �� *
ipString+6�   ]� 5       .+�5!      �� $
��Left=�6%   .+�5"      �� $��Rack:��6$   $��Center=�64   �� 5#      -   5        $
 rackTextBox��5#   &   $��Stretch=�6;   ��6Y   �� *rack+5$      �� 5%      $��Right=�6%   .+�5&      �� $��Slot:��6$   $��Center=�64   �� 5'      -   5        $
 slotTextBox��5'   &   $��Stretch=�6;   ��6Y   �� *slot+5(      5)      �� 5*      $��Communication Bit:��6   $��Center=�69   �� 5+      -   5        $
 communicationBitTextBox��5+      $��Stretch=�6?   ��6]   �� *CommunicationBit+�� 5,      -   5         ��Name$ saveConfigurationButtom��5,      $��Save Configuration��6>   $��Center=�6[   $��5q�6�   $��5q�6�   5-      5.      5/      � 52   
   $��2��6   $��1��6   $��3��6,   $��1,0,0,0q�6=   .+�53      �� 
����  �$��2q�6(   $ 20��6<   $��2q�6N   ._�54      �� .+�55      �� $��5.Read��6   $��Center=�6-   $��Comic Sans MS)�6J   $
��Bold"�6e   $��15(�6w   ]� 56      $��Stretch=�6    $��5q�6>   .+�57      �� -   5        $	��Add��57   !   $��5q�6/   6_   �� 58      -   5        $
 VariableNameTextBox��58   "   $��50��6?   $��5,0,5,0q�6N   ��6_   �� *ReadVariable+6�   59      &   System.Windows.Controls.DataGrid  5:      -	   5          Name$ readDataGrid��5:      $��Stretch=�65   $��5q�6S   "   VerticalScrollBarVisibility$
 Auto=�6^      AutoGenerateColumns .     
IsReadOnly .    RowHeaderWidth$ 0Q�6�      CanUserDeleteRows . $	��350��6$  ��6�   �� *
RVariables+   Columns 5;      0  *System.Windows.Controls.DataGridTextColumn  5<      ,  &System.Windows.Controls.DataGridColumn   Header$ Variable��61   -
   5        Variable��   Width5  /System.Windows.Controls.DataGridLengthConverter$ 1* 5<   U   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6`   �� *
Variable+6     5=      $ Value��61   -   5        	Value��$ 1* 5=   O    6Z   �� *Value+6v   5?      �� 5@      -   5        $ button��5@      $��Button��6-   $y�Hidden=�6S   5A      5B      5C      � 5D   
   $��1��6   $��1��6   $��2q�6,    5E      �� 5F      $��* �6#   �� 5G      $��* �6#   .+�5I      �� 
����  �$��2q�6(   $ 20��6<   $��5q�6N   $��1��6Y   ._�5J      �� .+�5K      �� $��4.Write��6   $��15(�6.   $��Center=�6<   $��Comic Sans MS)�6Y   $
��Bold"�6t   �� 5L      $��	Variable:��6   $��2q�60   6=   �� 5M      -   5        $
 WVariableTextBox��5M      $��2q�68   ��6C   �� *WriteVariable+�� 5N      $��Value:��6   $��2q�6-   �� 5O      -   5        $
 WValue��5O      $��2q�6.   ��69   �� *
WriteValue+6U   �� 5P      -   5        $ writeButton��5P      $��Write��62   $��2q�6B   $��30��6M   $��Center=�6s   $��	10,0,10,0q�6�   5Q      5R      �� 5S      
����  �$��2q�6(   $ 20��6<   $��	5,5,1.6,5q�6N   ._�5T      �� $��2q�6   .+�5U      �� $��2. Storage Configuration��6   $��15(�6?   $��Center=�6M   $��Comic Sans MS)�6j   $
��Bold"�6�   	� 5V      -   5         	�Name$ NoStorageButton��5V   "   $
��None��6;   $��2q�6J    d� 	IsChecked$
 TrueY�6U   6f   	� 5W      -   5        $ DBRadioButton��5W   "   $��	Data Base��69   $��2q�6M   6X   	� 5X      -   5        $ FileRadioButton��5X   "   $��
Local File��6;   $��2q�6Q   6\   �� 5Y      $��File Location:��6   {�65   ��  �� ElementName$ FileRadioButton�� �� Path$ 	IsChecked��� 5Z      -   5        $
 FileLocationTextBox��5Z       �� TextWrapping$
 Wrap=�6;   $��4q�6O    �� AcceptsReturn . {�6o   �� $ FileRadioButton��$ 	IsChecked��� 5[      -   5        $ 
ShowButton��5[      $��	Show Data��61   $��5q�6E   $��Center=�6P   $��5q�6m   6y   �� 5\      -   5        $ PickFileLocationButton��5\      $��PickLocation��6=   $��Center=�6�   $��5q�6�   {�6T   �� $ FileRadioButton��$ 	IsChecked�6�   5^      5_      5a      � 5c   
   $��Stretch=�6   $��2��6+   $��2��68   $��1��6I   $��2q�6Y   .+�5d      �� 
����  �$��2q�6(   $ 20��6<   $��5q�6N   ._�5e      �� .+�5f      �� $��6.Interpreter��6   $��15(�64   $��Center=�6B   $��Comic Sans MS)�6_   $
��Bold"�6z   �� 5g      
����ح�$ 10��64   $��	20,0,20,0q�6F   
����  �$��1q�6l   ._�5h      �� -   5        $
 interpreterTextBox��5h   "   $��10q�6>   
����ح�$��0q�6a   $��30��6u    . $
 Wrap=�6�   ��6�   �� *InterpreterText+6�   5i      �� 5j      $��Interpreter result:��6   $��Center=�6:   $��0,20,0,0q�6W   �� 5k      -   5        $
 interpretrResultTextBox��5k      $��Center=�6?   $	��100��6\    �� 
IsReadOnly . ��6z   �� *	IntResult+�� 5l      -   5         ��Name$ executeCheckBox��5l      $��Center=�68   $��	0,20,0,20q�6U   $!��Execute interpreter program��6h    6�   �� *ExecuteIntProgram+6�   5m      5n      5o      �� 5p   
   
����  �$��2q�6$   $ 20��68   $��Stretch=�6J   $��3��6f   $��5,4,5,5q�6s   ._�5q      �� $��5q�6   .+�5r      �� $��Center=�6   $��Comic Sans MS)�65   $
��Bold"�6P   $��3.Ping & Open Connection��6b   $��15(�6�   �� 5s      
����·�$ 10��63   $��	20,0,20,0q�6E   
����  �$��1q�6k   ._�5t      �� -   5         ��Name$ connectionTextBlock��5t       $��5q�6=   $��30��6I   ��6Y   �� *ConnectionString+5u      �� 5w      -   5        $ connectButton��5w      $��Connect��60   $��Center=�6B   $��5q�6_   $��5q�6k   6v   �� 5x      -   5        $ startTimeButton��5x      $��Start Readings��62   $��Center=�6K   $��5q�6h   $��5q�6t   6   5{      5|      5      5�      