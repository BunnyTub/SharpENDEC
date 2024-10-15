@app.route('/send_alert', methods=['POST'])
def send_alert():
    data = request.json
    nowTime = datetime.now(timezone.utc)
    sent = nowTime.strftime('%Y-%m-%dT%H:%M:%S-00:00')
    expire = nowTime + timedelta(hours=1)
    expire = expire.strftime('%Y-%m-%dT%H:%M:%S-00:00')
    sentforres = nowTime.strftime('%Y%m%dT%H%M%S')
    res = ''.join(random.choices(string.ascii_uppercase + string.digits, k=10))
    res = f"{res}{sentforres}"

    try: Cate = orgSAMEtoCAP[data['ORG']]
    except: Cate = "Other"

    EVENT = data['EVE']
    EVENT = EVENT.upper()
    EVENT = EVENT[:3]

    if data['SecondaryInfo'] is True:
        try: Cate_second = orgSAMEtoCAP[data['ORG_second']]
        except: Cate_second = "Other"
        EVENT_second = data['EVE_second']
        EVENT_second = EVENT_second.upper()
        EVENT_second = EVENT_second[:3]
        secondInfo = f"""
        <info>
            <language>{data['LANGUAGE_second']}</language>
            <category>{Cate_second}</category>
            <event>internal</event>
            <urgency>Unknown</urgency>
            <severity>Unknown</severity>
            <certainty>Unknown</certainty>
            <expires>{expire}</expires>

            <eventCode>
                <valueName>SAME</valueName>
                <value>{EVENT_second}</value>
            </eventCode>
            
            <senderName>QuantumENDEC Internal</senderName>
            <headline>{EVENT_second}</headline>
            <description>{data['broadcastText_second']}</description>
            
            <parameter>
                <valueName>layer:SOREM:1.0:Broadcast_Text</valueName>
                <value>{data['broadcastText_second']}</value>
            </parameter>

            <parameter>
                <valueName>EAS-ORG</valueName>
                <value>{data['ORG_second']}</value>
            </parameter>

            <area>
                <areaDesc>Specified Locations</areaDesc>
                <geocode>
                    <valueName>SAME</valueName>
                    <value>{data['FIPS_second']}</value>
                </geocode>
            </area>
        </info>
        """
    else: secondInfo = ""

    finalXML = f"""
    <alert>
        <sender>QuantumENDECinternal</sender>
        <identifier>{res}</identifier>    
        <sent>{sent}</sent>
        <status>Actual</status>
        <msgType>Alert</msgType>
        <note>This file is NOT supposed to find its way to the Pelmorex NAADs system, it's impossibe for it to be there... if it's there, it's not supposed to be there.</note>
        <note>This is an QuantumENDEC internal alert</note> 

        <info>
            <language>{data['LANGUAGE']}</language>
            <category>{Cate}</category>
            <event>internal</event>
            <urgency>Unknown</urgency>
            <severity>Unknown</severity>
            <certainty>Unknown</certainty>
            <expires>{expire}</expires>

            <eventCode>
                <valueName>SAME</valueName>
                <value>{EVENT}</value>
            </eventCode>
            
            <senderName>QuantumENDEC Internal</senderName>
            <headline>{EVENT}</headline>
            <description>{data['broadcastText']}</description>
            
            <parameter>
                <valueName>layer:SOREM:1.0:Broadcast_Text</valueName>
                <value>{data['broadcastText']}</value>
            </parameter>

            <parameter>
                <valueName>EAS-ORG</valueName>
                <value>{data['ORG']}</value>
            </parameter>

            <area>
                <areaDesc>Specified Locations</areaDesc>
                <geocode>
                    <valueName>SAME</valueName>
                    <value>{data['FIPS']}</value>
                </geocode>
            </area>
        </info>
        {secondInfo}
    </alert>
    """
    filenameXML = f"{sent.replace(':', '_')}I{res}.xml"
    print(f"Creating alert: {filenameXML}")
    with open(f"XMLqueue/{filenameXML}", "w", encoding="utf-8") as file: file.write(finalXML)
    return 'Alert XML created successfully.'