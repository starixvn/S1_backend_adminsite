
class Header extends React.Component {

    render() {
       return (<thead class="thead-light">
        <tr>
            <th scope="col">BankName</th>
            <th scope="col">AccountName</th>
            <th scope="col">AccountNo</th>
            <th scope="col">MinAmount</th>
            <th scope="col">MaxAmount</th>
            <th scope="col">Currency</th>
            <th scope="col">Status</th>
            <th scope="col">XXXX</th>
        </tr>
    </thead>
        );
    }
}


class Row extends React.Component {

    render() {
        let Data = this.props.Data;
        return (
        <tr>
            <td>{Data.bankName}</td>
            <td>{Data.accountName}</td>
            <td>{Data.accountNo}</td>
            <td>{Data.minAmount}</td>
            <td>{Data.maxAmount}</td>
            <td>{Data.currencyId}</td>
            <td>{Data.status}</td>
            <td>
                <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Edit"/>
            </td>
        </tr>
        );
    }
}


class RowEdit extends React.Component {

    render() {
        let Data = this.props.Data;
        return (
            <tr>
                <td><input type="text" name="bankName" value={Data.bankName}/></td>
                <td><input type="text" name="accountName" size ="10" value={Data.accountName}/></td>
                <td><input type="text" name="accountNo" maxlength="20" size ="20" value={Data.accountNo}/></td>
                <td><input type="text" name="minAmount" maxlength="4" size ="4" value={Data.minAmount} onkeyup="value=value.replace(/[^\d]/g,'') "/></td>
                <td><input type="text" name="maxAmount" maxlength="4" size ="4" value={Data.maxAmount} onkeyup="value=value.replace(/[^\d]/g,'') "/></td>
                <td><input type="text" name="currencyId" value={Data.currencyId} size ="4"/></td>
                <td><input name="status" type="checkbox" data-size="small"/></td>
                <td>
                    <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Save" />
                    <input type="button" class="btn btn-primary" onclick="UpdateStatus(1)" value="Cancel" />
                </td>
            </tr>
        );
    }
}

class Tag extends React.Component {
    constructor(props) {
        super(props);
        this.state = {Data: null};
    }

    componentDidMount() {
        let tt=this;
        axios.post('/BankList/GetBankList')
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    tt.setState({Data: result.data});
                    console.log(result);
                } else {
                    alert(result.message);
                }
            }).catch(function (error) {
                alert(error);
            });
    }
    
    render() {
        let rows = null;

        if(this.state.Data!=null) {
            rows = this.state.Data.map(function(obj) { 
                return <RowEdit Data={obj}/>;
             });
            
        }
        //;
        return (
            <div class="container">
                <table class="table table-striped">
                    <Header/>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        );
    }
}

ReactDOM.render(
    <Tag />,
    document.getElementById('Bank')
);