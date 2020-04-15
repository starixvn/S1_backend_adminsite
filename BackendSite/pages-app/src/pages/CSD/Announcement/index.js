import React, {useState, useEffect} from "react";
import ReactDOM from 'react-dom';
import Result from './Result';
import WebAPI from '../../../module//WebAPI';
import EditPanel from './Edit';
import moment from 'moment';
import qs from 'qs';

const defaultModel = {
    startTime:'',
    endTime:'',
    subject:{},
    content:{},
    status:0,
    isTest:0
}

const App = ()=>{
    const [stateDatas, updateDatas] = useState({
        Datas:null,
        PageInfo:null,
        SettingDatas:null,
        ShowSettingPanel:false,
        IsAdd:false
    });

    useEffect(()=>{
        let param = {
            Url: 'Message/GetAnnouncement',
            Data: `PageNumber=${1}`
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                updateDatas(
                    {
                        PageInfo:{
                            totalPages:res.data.totalPages,
                            pageNumber:1,
                            rowCount:res.data.rowCount
                        },
                        Datas:[...res.data.data]
                    }
                )
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        });
    },[]);

    const EditAnnouncement = (idx)=>{
        let param = {
            Url: 'Message/GetAnnouncementById',
            Data: `Id=${stateDatas.Datas[idx].id}`
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                updateDatas({
                    ...stateDatas,
                    SettingDatas:{
                        ...res.data.data
                    },
                    CurrentIdx:idx,
                    ShowSettingPanel:true
                });
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        });
    }

    const SetAnnouncement = (datas, isAdd)=>{
        let newDatas = {};
        Object.keys(datas).forEach(key=>{
            newDatas[key.charAt(0).toUpperCase() + key.slice(1)] = datas[key];
        });
        
        let lastUpdTime = datas.lastUpdater ? datas.lastUpdateTime : new Date();
        let param = {
            Url: isAdd ? 'Message/AddAnnouncement' : 'Message/UpdateAnnouncement',
            Data:  qs.stringify({
                Datas:{
                    ...newDatas, 
                    Subject:JSON.stringify(datas.subject),
                    Content:JSON.stringify(datas.content),
                    StartTime:moment(datas.startTime).toJSON(),
                    EndTime:moment(datas.endTime).toJSON(),
                    LastUpdateTime:moment(lastUpdTime).toJSON(),
                    CreateTime:moment(datas.createTime).toJSON()
                },
                _StartTime:moment(datas.startTime).unix(),
                _EndTime:moment(datas.endTime).unix(),
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                let tmpDatas = [...stateDatas.Datas];
                let appendDatas = isAdd ? {creator:res.data.data.creator, createTime:res.data.data.createTime, id:res.data.data.id} : {lastUpdater:res.data.data.lastUpdater, lastUpdateTime:res.data.data.lastUpdateTime}

                if(isAdd){
                    tmpDatas.push({
                        ...datas,
                        subject:datas.subject.en,
                        content:datas.content.en,
                        ...appendDatas
                    });

                    tmpDatas = tmpDatas.sort((a, b)=>{return b.id - a.id});
                }else{
                    tmpDatas[stateDatas.CurrentIdx] = {
                        ...datas,
                        subject:datas.subject.en,
                        content:datas.content.en,
                        ...appendDatas
                    }
                }

                updateDatas({
                    ...stateDatas,
                    Datas:tmpDatas,
                    SettingDatas:null,
                    ShowSettingPanel:false
                });
            }else{
                alert(res.data.message ?? 'failed to get data.');
            }
        });
    }

    const DeleteAnnouncement = (idx)=>{
        if(window.confirm(`Delete Announcement which Id is ${stateDatas.Datas[idx].id} ?`)){
            let param = {
                Url: 'Message/DeleteAnnouncement',
                Data: `Id=${stateDatas.Datas[idx].id}`
            };
            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    let tmpDatas = [...stateDatas.Datas];
                    tmpDatas.splice(idx, 1);
                    updateDatas({
                        ...stateDatas,
                        Datas:[...tmpDatas]
                    });
                }else{
                    alert(res.data.message ?? 'failed to get data.');
                }
            });
        }
    }

    return(
        <div className="container-fluid">
            <h4 className="page-name">{`${stateDatas.ShowSettingPanel ? stateDatas.IsAdd ? 'Add ': 'Edit ' : ''}Announcement`}</h4>
            {
                stateDatas.ShowSettingPanel ? (
                    <EditPanel 
                        ClickBack={()=>{updateDatas({...stateDatas, ShowSettingPanel:false, IsAdd:false})}} 
                        Datas={stateDatas.SettingDatas}
                        IsAdd={stateDatas.IsAdd}
                        SetAnnouncement={SetAnnouncement}
                    />
                ) : (
                    <>
                        <button className="btn btn-add" style={{marginBottom: '20px'}} onClick={()=>{updateDatas({...stateDatas, SettingDatas:{...defaultModel}, ShowSettingPanel:true, IsAdd:true})}}>Add Announcement</button>
                        {stateDatas.Datas === null ? null : <Result Datas={stateDatas.Datas} PageInfo={stateDatas.PageInfo} ClickEdit={EditAnnouncement} ClickDelete={DeleteAnnouncement}/>}
                    </>
                )
            }
        </div>
    )
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}