export class Notes extends React.Component {
    constructor(props) {
        super(props);
        this.state = {note:""};
    }

    componentDidMount() {
        if(this.props.Notes==null) {
            this.props.GetCustNotes();
        }
    }

    handleSubmit(event) {
        event.preventDefault();

        this.props.AddCustNote(this.state.note);
        //this.props.GetTpTransferInfoByCust(1);
    }
    
    onChange(event) {
        let obj=this.state;
        obj[event.target.name]=event.target.value;
        this.setState(obj);
    }


    render() {
        let notes = this.props.Notes;
        let notesList = null;
        if(notes!=null) {
            notesList = <ul>{notes.map( note => <li>
                                <p className="content">{note.note}</p>
                                <p id="fonts-wrapper">{/*<i className="fa fa-edit"></i><i className="fa fa-trash"></i>*/}
                                </p>
                                <input className="edit-note content-edit" type="text"/>
                                <p className="author"><span className="user">{note.operator}</span><span className="date">{moment(note.createTime).format('l LTS (Z)')}</span></p>
                            </li>)}</ul>;
        }
        return (
            <div>
                <div className="wrapper-note">
                    <div id="add-notes">
                        <form id="add" className="form-add" onSubmit={this.handleSubmit.bind(this)}>
                            <input className="add-note-field" type="text" onChange={this.onChange.bind(this)} placeholder="Add a note..." name="note" value={this.state.note}/>
                            <input className="btn btn-add" type="submit" value="Add"/>
                        </form>
                    </div>
                    <div className="note-list">
                        {notesList}
                    </div>
                </div>                
            </div>
        )
    }
}
