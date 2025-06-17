using System;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {
        private void SpeakMessage()
        {
            try
            {
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
                    _speechSynthesizer.SelectVoiceByHints(_selectedVoice == "Female" ? VoiceGender.Female : VoiceGender.Male);
                    _speechSynthesizer.Volume = 100;
                    _speechSynthesizer.Rate = 0;
                    _speechSynthesizer.SpeakAsync(lastAIBubble.MessageText);
                }
                else
                {
                    MessageBox.Show("No AI messages to read.", "No Messages", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error speaking message: {ex.Message}", "TTS Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
