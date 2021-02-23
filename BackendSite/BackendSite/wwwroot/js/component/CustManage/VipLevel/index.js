import Setting from './Setting';
import Log from './Log';
import { getUrlParameter } from '../../commJS/common';

const Panel = () => {

    let defaultData = {
        manually: 0,
        vipLevel: 0
    }
    const [stateLevelSetting, setLevelSetting] = React.useState();
    const [stateDatas, setDatas] = React.useState();
    const [stateUpdateDatas, setUpdateDatas] = React.useState();
    const [stateLevelLog, setLevelLog] = React.useState();
    const [stateisEdit, setisEdit] = React.useState(false);

    let CustId = getUrlParameter("custId");
    let iCustId = parseInt(CustId);
    React.useEffect(() => {

        axios.post('/api/VipLevel/GetVipLevelSetting', { CustId: iCustId }).then((res) => {
            let result = res.data;
            if (result.errorCode == 0) {
                setLevelSetting(result.data);
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });


        GetVipLevelInfo();
        GetVipLevelLog();
    }, []);

    const GetVipLevelInfo = () => {
        axios.post('/api/VipLevel/GetVipLevelInfo', { CustId: iCustId }).then((res) => {
            let result = res.data;
            if (result.errorCode == 0) {
                if (result.data) {
                    setDatas(result.data);
                } else {
                    setDatas(defaultData);
                }
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });
    }

    const GetVipLevelLog = () => {
        axios.post('/api/VipLevel/GetVipLevelLog', { CustId: iCustId }).then((res) => {
            let result = res.data;
            if (result.errorCode == 0) {
                setLevelLog(result.data);
            } else {
                alert(result.message);
            }
        }).catch((error) => {
            alert(error);
        });
    }

    const onClickEdit = (isEdit) => {
        setisEdit(!isEdit);
        if (!isEdit) {
            setUpdateDatas(stateDatas);
        } else {
            setUpdateDatas();
        }
    }

    const onChange = (value, dataColumn) => {
        setUpdateDatas({ ...stateUpdateDatas, [dataColumn]: value });
    }

    const editVIPUpdate = () => {
        console.log(stateDatas);
        console.log(stateUpdateDatas);
        if (confirm('Are you sure change Vip Level Info ?')) {
            if (stateDatas.vipLevel != stateUpdateDatas.vipLevel) {
                axios.post('/api/VipLevel/SetVipLevel', { CustId: iCustId, VipLevel: parseInt(stateUpdateDatas.vipLevel) }).then((res) => {
                    let result = res.data;
                    if (result.errorCode == 0) {
                        GetVipLevelInfo();
                        GetVipLevelLog();
                        setisEdit(false);
                    } else {
                        alert(result.message);
                    }
                }).catch((error) => {
                    alert(error);
                });
            }
        }
    }

    const editManuallyUpdate = (ManuallyUpdate) => {
        console.log(stateDatas);
        console.log(ManuallyUpdate);

        if (confirm('Are you sure change Vip Level Manually ?')) {
            if (stateDatas.manually != ManuallyUpdate && stateDatas.vipLevel > 0) {
                axios.post('/api/VipLevel/SetVipLevelManually', { CustId: iCustId, Manually: ManuallyUpdate ? 1 : 0 }).then((res) => {
                    let result = res.data;
                    if (result.errorCode == 0) {
                        GetVipLevelInfo();
                        GetVipLevelLog();
                        setisEdit(false);
                    } else {
                        alert(result.message);
                    }
                }).catch((error) => {
                    alert(error);
                });
            } else {
                alert("VIP level must be set before manual adjustment");
            }
        }
    }


    return (
        <>
            <div className="filter" style={{ display: "block" }}>
                <span className="label">Cust Level</span>
                {
                    stateDatas === undefined || stateLevelSetting === undefined ? null : <Setting Data={stateDatas} UpdateDatas={stateUpdateDatas} Setting={stateLevelSetting} isEdit={stateisEdit} onClickEdit={onClickEdit} onChange={onChange} editVIPUpdate={editVIPUpdate} editManuallyUpdate={editManuallyUpdate} />
                }
            </div>
            <div className="filter" style={{ display: "block" }}>
                <span className="label">Cust Level Log</span>
                {
                    stateLevelLog === undefined || stateLevelSetting === undefined ? null : <Log Logs={stateLevelLog} Setting={stateLevelSetting} />
                }
            </div>
        </>
    )
}

export default Panel;