import React, {useState} from "react";
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';

const EditPanel = ()=>{
    const [stateData, updateData] = useState({
        userName:'',
        subject:{},
        content:{}
    });
    
    let allLang = ['en', 'cs', 'ch', 'th', 'id', 'vn'];

    const EditFiled = (v, datacolumn, subDatas)=>{
        if(subDatas && subDatas.lang){
            updateData({
                ...stateData,
                [datacolumn]:{
                    ...stateData[datacolumn],
                    [subDatas.lang]:v
                }
            });
        }else{
            updateData({
                ...stateData,
                [datacolumn]: v
            });
        }
    }

    const SetMsg = ()=>{
        let msg = "";
        let check = true;
        if(!stateData.subject || !stateData.subject.en){
            check = false;
            msg = "Please input en subject.";
        }else if(!stateData.content || !stateData.content.en){
            check = false;
            msg = "Please input en content.";
        }

        if(check){
            let msg = {};
            allLang.forEach(lang=>{
                msg[lang] = {
                    Title:stateData.subject.en,
                    Content:stateData.content.en
                }
            });

            Object.keys(stateData.subject).forEach(sKey => {
                if(stateData.subject[sKey]){
                    if(msg[sKey]){
                        msg[sKey].Title = stateData.subject[sKey];
                    }else{
                        msg[sKey] = {
                            Title:stateData.subject[sKey],
                            Content:''
                        }
                    }
                }
            });

            Object.keys(stateData.content).forEach(sKey => {
                if(stateData.content[sKey]){
                    if(msg[sKey]){
                        msg[sKey].Content = stateData.content[sKey];
                    }else{
                        msg[sKey] = {
                            Title:'',
                            Content:stateData.content[sKey]
                        }
                    }
                }
            });

            let param = {
                Url: 'Message/SendMessageBatch',
                Data: qs.stringify({
                    personalMsg:msg,
                    custList:stateData.userName.split('\n').map(u=>{return u.replace(' ', '')})
                })
            };
    
            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0){
                    updateData({
                        userName:'',
                        subject:{},
                        content:{}
                    });
                    alert('sent successfully');
                } else {
                    alert(res.data.message ?? 'failed.');
                }
            });
        }else{
            alert(msg);
        }
    }

    let langPanels = allLang.map(lang=>{
        return(
            <OuterPanel
                key={`set-panel-${lang}`}
                HeaderContent={<span style={{color:'white'}}>{lang}</span>}
                Content={<SettingContent subject={stateData.subject[lang] === undefined ? '' : stateData.subject[lang]} content={stateData.content[lang] === undefined ? '' : stateData.content[lang]} lang={lang} EditFiled={EditFiled}/>}
            />
        );
    });

    return(
        <>
            <div className="form-group">
                <label>UserName</label>
                <textarea style={{'width':'500px'}} className="form-control" rows="4" value={stateData.userName} onChange={(e)=>{ EditFiled(e.target.value, 'userName')}}></textarea>
            </div>
            {langPanels}
            <button className="btn btn-add" style={{marginTop: '20px', marginBottom: '40px', float: 'right'}} onClick={SetMsg}>Submit</button>
        </>
    );
}

const OuterPanel = (props)=>{
    return(
        <div className="panel level-1 mt-20">
            <a style={{"justifyContent": "space-between"}} className="panel-title">
                {props.HeaderContent}
            </a>
            <div className="panel-body">
                <div className="collapse show">
                    {props.Content}
                </div>
            </div>
        </div>
    )
}

const SettingContent = (props)=>{
    return(
        <div className="panel-body">
            <div className="form-group">
                <label>Subject</label>
                <input type="text" className="form-control" value={props.subject} onChange={(e)=>{ props.EditFiled(e.target.value, 'subject', {lang:props.lang})}}/>
            </div>
            <div className="form-group">
                <label>Content</label>
                <textarea className="form-control" rows="8" value={props.content} onChange={(e)=>{ props.EditFiled(e.target.value, 'content', {lang:props.lang})}}></textarea>
            </div>
        </div>
    )
}

export default EditPanel;