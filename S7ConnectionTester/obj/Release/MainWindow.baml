   M S B A M L   `   `   `  ���� M  IS7ConnectionTester, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:S7ConnectionTesterS7ConnectionTester  #    S7ConnectionTester.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:S7ConnectionTester       � Title$  PLC Communications��
����İ�$��Franklin Gothic Medium)� � WindowStartupLocation$ CenterScreen=� � SizeToContent$ WidthAndHeight=�$	��500��$	��770�� � Icon$ Resources\osa_ics_plc.ico��.���  � RowDefinitions �� $��* ��� $��9* �.+�� 
�������.+��� $��0��$��Resources/put3_logo.png��]� $��10,10,10.2,10.4q�$��1��$��1��.+�� $
��Left=�$	��220��$��Stretch=�.+��� 
����  �$��2q� �� CornerRadius$ 20��._��� .+��� $��1.Plc driver data��$��15(�$��Center=�$��Comic Sans MS)�$
��Bold"�$��	0,10,0,10q��� $��PLC processor type:��$��Center=�$��0,10,0,0q�$	��Top=��� -    ��Name$ CpuTypeComboBox��$��Center=�$	��100�� �� DisplayMemberPath$	 Key�� �� SelectedValuePath$ Value������ *	cpuType+�� $��IP address:��$��0,10,0,0q�$��Center=��� -   	 ��Name$	 	ipTextBox��$��Center=�$	��150��
�������
 �� TextAlignment$
 Center=����� *
ipString+]� $��
20,10,20,0q�.+��� $
��Left=�.+��� $��Rack:��$��Center=��� -   $	 rackTextBox��$��50��$��Center=����� *rack+�� $��Right=�.+��� $��Slot:��$��Center=��� -   $	 slotTextBox��$��50��$��Center=����� *slot+�� $��Communication Bit:��$��Center=�$��0,10,0,0q��� -   $	 communicationBitTextBox��$��Center=�$	��100������ *CommunicationBit+�� -    ��Name$ saveConfigurationButtom��$��Save Configuration��$��	0,20,0,30q�$��Center=�$��30��� $��20,0,0,0q�$
��Left=�$��Stretch=�$	��300�� �� $
��170* ��� $
��260* �.+��� 
����  �$��2q�$ 20��$	��Top=�$��0��$��0q�$	��161��._��� .+��� $��15(�$��Center=�$��Comic Sans MS)�$
��Bold"�.��2.Ping & Open Connection�� 
����·�$ 10��$��	20,0,20,0q�
����  �$��1q�._��� -    ��Name$ connectionTextBlock��$��10q�$��30������ *ConnectionString+�� -   $ connectButton��$��Connect��$��Center=�$��35��$��70��$��20,20,20,20q�]� $��1��$��Stretch=�$��0,10,0,0q�.+��� 
����  �$��2q�$ 20��._��� .+��� $��3.Read��$��15(�$��Center=�$��Comic Sans MS)�$
��Bold"�]� $��Center=�$��
10,10,10,0q�.+��� -	   $	 VariableNameTextBox��$��80��$��5,0,5,0q����� *ReadVariable+�� -
   $	��Add��$��40��$��10,0,5,0q�&   System.Windows.Controls.DataGrid  -     Name$ readDataGrid��$��Center=�$	��150��$��10,10,10,10q�"   VerticalScrollBarVisibility$
 Auto=�   AutoGenerateColumns .     
IsReadOnly .    RowHeaderWidth$ 0Q�$	��180��   CanUserDeleteRows . ���� *
RVariables+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Variable��-   Variable��   Width5  /System.Windows.Controls.DataGridLengthConverter$ 1* 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *
Variable+  $ Value��-   	Value��$ 1*  �� *Value+�� 
����  �$��2q�$ 20��$��10,0,5,0q�$��1��._��� .+��� $��4.Write��$��15(�$��Center=�$��Comic Sans MS)�$
��Bold"��� $��	Variable:��$��
10,10,10,0q��� -   $	 WVariableTextBox��$��	10,0,10,0q����� *WriteVariable+�� $��Value:��$��
10,10,10,0q��� -   $	 WValue��$��	10,0,10,0q����� *
WriteValue+�� -   $ writeButton��$��Write��$��10,20,10,10q�$��30��� $	��Top=�.+��� 
����  �$��2q�$ 20��$	��Top=�$��10,0,0,0q�$��0��._��� .+��� $��5.Interpreter Tester��$��15(�$��Center=�$��Comic Sans MS)�$
��Bold"��� 
����ح�$ 10��$��	20,0,20,0q�
����  �$��1q�._��� -   $	 interpreterTextBox��$��10q�
����ح�$��0q�$��30�� �� AcceptsReturn .  �� TextWrapping$
 Wrap=����� *InterpreterText+�� $��Interpreter result:��$��Center=�$��0,20,0,0q��� -   $	 interpretrResultTextBox��$��Center=�$	��100�� �� 
IsReadOnly . ���� *	IntResult+�� -    ��Name$ executeCheckBox��$��Center=�$��	0,20,0,20q�$!��Execute interpreter program�� d� 	IsChecked �� *ExecuteIntProgram+