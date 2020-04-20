const AddPanel = (props)=>{
    const [stateData, updateData] = React.useState({
        title:'',
        content:''
    });

    const ClickSend = ()=>{
        props.ClickSend(stateData);
    }

    return(
        <form>
            <button type="button" className="btn btn-back" onClick={props.ClickBack} style={{marginBottom:'1rem'}}>Back</button>
            <div style={{marginBottom:'1rem'}}>
                <p style={{marginBottom:0}} className="label">User Prefer Language:</p>
                <span>{props.UserLang}</span>
            </div>
            <div className="form-group">
                <label>Subject</label>
                <input type="text" className="form-control" onBlur={(e)=>{updateData({...stateData, title:e.target.value})}}/>
            </div>
            <div className="form-group">
                <label>Content</label>
                <textarea className="form-control" rows="8" onBlur={(e)=>{updateData({...stateData, content:e.target.value})}}></textarea>
            </div>
            <button type="button" className="btn btn-add" onClick={ClickSend}>Send</button>
        </form>
    )
}

export default AddPanel;