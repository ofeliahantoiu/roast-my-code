using System;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private void SpeakMessage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastAIMessage))
                {
                    // Play sound effect first
                    PlaySoundEffect();
                    
                    // Small delay before speech
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() => Speak(_lastAIMessage, _selectedVoice)));
                        }
                        else
                        {
                            Speak(_lastAIMessage, _selectedVoice);
                        }
                    });
                }
                else
                {
                    // Fallback: try to find the last AI message in chat
                    ChatMessageBubble? lastAIBubble = null;
                    foreach (Control control in chatAreaPanel.Controls)
                    {
                        if (control is ChatMessageBubble bubble && bubble.Role == "assistant")
                        {
                            lastAIBubble = bubble;
                        }
                    }

                    if (lastAIBubble != null)
                    {
                        // Play sound effect first
                        PlaySoundEffect();
                        
                        // Small delay before speech
                        Task.Delay(100).ContinueWith(_ =>
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() => Speak(lastAIBubble.MessageText, _selectedVoice)));
                            }
                            else
                            {
                                Speak(lastAIBubble.MessageText, _selectedVoice);
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("No AI messages to read.", "No Messages", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error speaking message: {ex.Message}", "TTS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
